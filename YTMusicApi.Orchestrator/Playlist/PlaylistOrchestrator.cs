using YTMusicApi.Model.Integration;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.PlaylistTrack;
using YTMusicApi.Model.Track;
using YTMusicApi.Model.UserPlaylist;
using YTMusicApi.Model.YouTube;
using YTMusicApi.Shared.Optimization;
using AutoMapper;

namespace YTMusicApi.Orchestrator.Playlist
{
    public class PlaylistOrchestrator : IPlaylistOrchestrator
    {
        private readonly IYouTubeRepository _youTubeRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IPlaylistTrackOrchestrator _playlistTrackOrchestrator;
        private readonly IUserPlaylistOrchestrator _userPlaylistOrchestrator;

        private readonly IOptimizerClient _optimizerClient;
        private readonly IMapper _mapper;

        public PlaylistOrchestrator(
            IYouTubeRepository youTubeRepository,
            IPlaylistRepository playlistRepository,
            IPlaylistTrackOrchestrator playlistTrackOrchestrator,
            IUserPlaylistOrchestrator userPlaylistOrchestrator,
            IOptimizerClient optimizerClient,
            IMapper mapper)
        {
            _youTubeRepository = youTubeRepository;
            _playlistRepository = playlistRepository;
            _playlistTrackOrchestrator = playlistTrackOrchestrator;
            _userPlaylistOrchestrator = userPlaylistOrchestrator;
            _optimizerClient = optimizerClient;
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
                throw new ArgumentNullException("Playlist not found on YouTube Music.");

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
                throw new ArgumentNullException("Playlist not found in the database.");
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
                throw new ArgumentNullException("Playlist not found on YouTube Music.");
            }
            var updatedPlaylist = await _playlistRepository.UpdatePlaylistAsync(playlistDto);
            
            await _playlistTrackOrchestrator.UpdateTracksFromPlaylistAsync(playlistDto.PlaylistId);
           
            return updatedPlaylist;
        }

        public async Task<List<TrackDto>> GetOptimizedTracksAsync(string playlistId, TimeSpan timeLimit, int? maxTracks, OptimizationAlgorithmType algorithm, double genreWeight, string? startTrackId)
        {
            var sourceTracks = await _playlistTrackOrchestrator.GetTracksForPlaylistAsync(playlistId);
            if (sourceTracks == null || !sourceTracks.Any())
            {
                throw new ArgumentNullException("Source playlist is empty or not found.");
            }
            
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

            var optimizationResult = await _optimizerClient.OptimizePlaylistAsync(optimizationSettingsDto);
            if (!optimizationResult.Success)
            {
                 throw new InvalidOperationException(optimizationResult.ErrorMessage ?? "Optimization failed.");
            }

            var trackDictionary = sourceTracks.ToDictionary(t => t.TrackId);
            var optimizedTracks = new List<TrackDto>();

            foreach (var trackId in optimizationResult.OrderedTrackIds)
            {
                if (trackDictionary.TryGetValue(trackId, out var track))
                {
                    optimizedTracks.Add(track);
                }
            }

            return optimizedTracks;
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
    }
}
