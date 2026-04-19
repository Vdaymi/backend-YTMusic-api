﻿using System.Text.Json;
 using YTMusicApi.Model.Integration;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.PlaylistTrack;
using YTMusicApi.Model.Track;
using YTMusicApi.Model.UserPlaylist;
using YTMusicApi.Model.YouTube;
using YTMusicApi.Shared.Optimization;
using AutoMapper;
using YTMusicApi.Model.MessageBroker;
using YTMusicApi.Model.Optimization;
using YTMusicApi.Shared.MessageBroker;
using YTMusicApi.Shared.Messaging;

namespace YTMusicApi.Orchestrator.Playlist
{
    public class PlaylistOrchestrator : IPlaylistOrchestrator
    {
        private readonly IYouTubeRepository _youTubeRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IOptimizationRepository _optimizationRepository;
        private readonly IPlaylistTrackOrchestrator _playlistTrackOrchestrator;
        private readonly IUserPlaylistOrchestrator _userPlaylistOrchestrator;
        private readonly IMapper _mapper;

        public PlaylistOrchestrator(
            IYouTubeRepository youTubeRepository,
            IPlaylistRepository playlistRepository,
            IOptimizationRepository optimizationRepository,
            IPlaylistTrackOrchestrator playlistTrackOrchestrator,
            IUserPlaylistOrchestrator userPlaylistOrchestrator,
            IMapper mapper)
        {
            _youTubeRepository = youTubeRepository;
            _playlistRepository = playlistRepository;
            _optimizationRepository = optimizationRepository;
            _playlistTrackOrchestrator = playlistTrackOrchestrator;
            _userPlaylistOrchestrator = userPlaylistOrchestrator;
            _mapper = mapper;
        }

        public async Task<PlaylistDto> PostPlaylistAsync(string playlistId, Guid userId)
        {
            var existingPlaylist = await _playlistRepository.GetByIdPlaylistAsync(playlistId);
            if (existingPlaylist != null)
            {
                await _userPlaylistOrchestrator.PostPlaylistToUserAsync(userId, playlistId);
                return existingPlaylist;
            }
            var youTubePlaylist = await _youTubeRepository.GetPlaylistAsync(playlistId);
            if (youTubePlaylist == null)
                throw new KeyNotFoundException("Playlist not found on YouTube Music.");

            var savedPlaylist = await _playlistRepository.PostPlaylistAsync(youTubePlaylist);

            await _userPlaylistOrchestrator.PostPlaylistToUserAsync(userId, playlistId);

            await _playlistTrackOrchestrator.UpdateTracksFromPlaylistAsync(youTubePlaylist.PlaylistId);

            return savedPlaylist;
        }

        public async Task<PlaylistDto> GetByIdPlaylistAsync(string playlistId)
        {
            var playlistDto = await _playlistRepository.GetByIdPlaylistAsync(playlistId);
            if (playlistDto == null)
            {
                throw new KeyNotFoundException("Playlist not found in the database.");
            }
            return playlistDto;
        }

        public async Task<PlaylistDto> UpdatePlaylistAsync(string playlistId) 
        {
            var existingPlaylist = await GetByIdPlaylistAsync(playlistId);
            if (existingPlaylist.Source == PlaylistSource.Optimized) 
                return existingPlaylist;

            var playlistDto = await _youTubeRepository.GetPlaylistAsync(playlistId);
            if (playlistDto == null)
            {
                throw new KeyNotFoundException("Playlist not found on YouTube Music.");
            }
            var updatedPlaylist = await _playlistRepository.UpdatePlaylistAsync(playlistDto);
            
            await _playlistTrackOrchestrator.UpdateTracksFromPlaylistAsync(playlistDto.PlaylistId);
           
            return updatedPlaylist;
        }

        public async Task<Guid> InitiateOptimizationAsync(string playlistId, Guid userId, TimeSpan timeLimit, int? maxTracks, OptimizationAlgorithmType algorithm, double genreWeight, string? startTrackId)
        {
            var sourceTracks = await _playlistTrackOrchestrator.GetTracksForPlaylistAsync(playlistId);
            if (sourceTracks == null || !sourceTracks.Any())
            {
                throw new KeyNotFoundException("Source playlist is empty or not found.");
            }
             
            var taskId = Guid.NewGuid();
 
            var taskDto = new OptimizationTaskDto
            {
                TaskId = taskId,
                UserId = userId,
                PlaylistId = playlistId,
                Status = OptimizationTaskStatus.Pending
            };

            var optimizationSettingsDto = new OptimizationSettingsDto
            {
                SourceTracks = _mapper.Map<List<TrackOptimizationDto>>(sourceTracks),
                TimeLimit = timeLimit,
                MaxTracks = maxTracks ?? sourceTracks.Count,
                Algorithm = algorithm,
                GenreWeight = genreWeight,
                YearWeight = 1 - genreWeight,
                StartTrackId = startTrackId
            };

            var command = new OptimizePlaylistCommand
            {
                TaskId = taskId,
                Settings = optimizationSettingsDto
            };

            var outboxMessageDto = new OutboxMessageDto
            {
                Id = Guid.NewGuid(),
                Type = nameof(OptimizePlaylistCommand),
                Payload = JsonSerializer.Serialize(command),
                Exchange = MessagingConstants.OptimizationExchange,
                RoutingKey = MessagingConstants.OptimizeCommandRoutingKey
            };

            await _optimizationRepository.CreateTaskAndOutboxMessageAsync(taskDto, outboxMessageDto);
 
            return taskId;
        }
        
        public async Task<PlaylistDto> PostOptimizedPlaylistAsync(Guid userId, string title, string channelTitle, List<string> trackIds, TimeSpan targetDuration, OptimizationAlgorithmType algorithm, double genreWeight)
        {
            string newPlaylistId = "OP" + Guid.NewGuid().ToString("N");

            var playlistDto = new PlaylistDto
            {
                PlaylistId = newPlaylistId,
                Title = title,
                ChannelTitle = channelTitle,
                ItemCount = trackIds.Count,
                Source = PlaylistSource.Optimized
            };
            var savedPlaylist = await _playlistRepository.PostPlaylistAsync(playlistDto);

            await _userPlaylistOrchestrator.PostPlaylistToUserAsync(userId, newPlaylistId);
            await _playlistTrackOrchestrator.PostOptimizedTracksAsync(newPlaylistId, trackIds);

            var settingsDto = new PlaylistSettingDto
            {
                PlaylistId = newPlaylistId,
                TargetDuration = targetDuration,
                Algorithm = algorithm,
                GenreWeight = genreWeight
            };
            await _playlistRepository.PostPlaylistSettingsAsync(settingsDto);

            return savedPlaylist;
        }

        public async Task<byte[]> GetCsvExportAsync(string playlistId)
        {
            var tracks = await _playlistTrackOrchestrator.GetTracksForPlaylistAsync(playlistId);
            if (tracks == null || !tracks.Any())
            {
                throw new KeyNotFoundException("Playlist is empty or not found.");
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("title,artist,album,isrc");

            foreach (var track in tracks)
            {
                var title = track.Title?.Replace("\"", "\"\"") ?? "Unknown";
                var channel = track.ChannelTitle?.Replace("\"", "\"\"") ?? "Unknown"; 
            
                sb.AppendLine($"\"{title}\",\"{channel}\",,");
            }
            var csvContent = sb.ToString();
            var preamble = System.Text.Encoding.UTF8.GetPreamble();
            var contentBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
            
            return preamble.Concat(contentBytes).ToArray();
        }
    }
}
