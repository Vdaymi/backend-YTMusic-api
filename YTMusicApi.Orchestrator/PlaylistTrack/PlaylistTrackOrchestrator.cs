using YTMusicApi.Model.PlaylistTrack;
using YTMusicApi.Model.Track;
using YTMusicApi.Model.YouTube;

namespace YTMusicApi.Orchestrator.PlaylistTrack
{
    public class PlaylistTrackOrchestrator : IPlaylistTrackOrchestrator
    {
        private readonly IPlaylistTrackRepository _playlitTrackRepository;
        private readonly IYouTubeRepository _youTubeRepository;
        private readonly ITrackRepository _trackRepository;
        public PlaylistTrackOrchestrator(
            IPlaylistTrackRepository playlitTrackRepository,
            IYouTubeRepository youTubeRepository,
            ITrackRepository trackRepository)
        {
            _playlitTrackRepository = playlitTrackRepository;
            _youTubeRepository = youTubeRepository;
            _trackRepository = trackRepository;
        }

        public async Task<List<TrackDto>> GetTracksForPlaylistAsync(string playlistId)
        {
            var trackDtos = await _playlitTrackRepository.GetTracksByPlaylistAsync(playlistId);
            if (trackDtos == null || !trackDtos.Any())
            {
                throw new KeyNotFoundException("Tracks not found for the given playlist.");
            }
            return trackDtos;
        }

        public async Task<PlaylistTrackDto> PostTrackToPlaylistAsync(string playlistId, string trackId)
        {
            var playlistTrackDto = new PlaylistTrackDto();
            playlistTrackDto.PlaylistId = playlistId;
            playlistTrackDto.TrackId = trackId;

            return await _playlitTrackRepository.PostTrackToPlaylistAsync(playlistTrackDto);
        }

        public async Task<PlaylistTrackDto> DeleteTrackFromPlaylistAsync(string playlistId, string trackId)
        {
            var playlistTrackDto = new PlaylistTrackDto();
            playlistTrackDto.PlaylistId = playlistId;
            playlistTrackDto.TrackId = trackId;
            
            return await _playlitTrackRepository.DeleteTrackFromPlaylistAsync(playlistTrackDto);
        }

        public async Task UpdateTracksFromPlaylistAsync(string playlistId)
        {
            var youTubeTrackIds = await _youTubeRepository.GetTrackIdsFromPlaylistAsync(playlistId);

            var dbTrackIds = await _playlitTrackRepository.GetTrackIdsByPlaylistAsync(playlistId);

            var toAdd = youTubeTrackIds.Except(dbTrackIds).ToList();
            var toRemove = dbTrackIds.Except(youTubeTrackIds).ToList();

            if (toAdd.Any())
            {
                var missingTrackDtos = await _youTubeRepository.GetTracksAsync(toAdd);
                foreach (var missingTrackDto in missingTrackDtos)
                {
                    var exists = await _trackRepository.GetByIdTrackAsync(missingTrackDto.TrackId);
                    if (exists == null)
                        await _trackRepository.PostTrackAsync(missingTrackDto);
                     
                    await PostTrackToPlaylistAsync(playlistId, missingTrackDto.TrackId);

                }
            }
            if (toRemove.Any())
            {
                foreach (var trackId in toRemove)
                    await DeleteTrackFromPlaylistAsync(playlistId, trackId);
            }
        }

        public async Task<List<TrackDto>> UpdateTracksDataFromPlaylist(string playlistId)
        {
            var dbTrackIds = await _playlitTrackRepository.GetTrackIdsByPlaylistAsync(playlistId);
            if(dbTrackIds == null || !dbTrackIds.Any())
            {
                throw new KeyNotFoundException("Playlist not found in the database.");
            }
            var trackDtos = await _youTubeRepository.GetTracksAsync(dbTrackIds);

            foreach (var trackDto in trackDtos)
                await _trackRepository.UpdateTrackAsync(trackDto);

            return trackDtos;
        }

        public async Task PostOptimizedTracksAsync(string playlistId, List<string> trackIds)
        {
            int orderIndex = 0;
            foreach (var trackId in trackIds)
            {
                var playlistTrackDto = new PlaylistTrackDto
                {
                    PlaylistId = playlistId,
                    TrackId = trackId,
                    OrderIndex = orderIndex++ 
                };
                await _playlitTrackRepository.PostTrackToPlaylistAsync(playlistTrackDto);
            }
        }
    }
}
