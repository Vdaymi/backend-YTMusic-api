using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.PlaylistTrack;
using YTMusicApi.Model.Track;
using YTMusicApi.Model.YouTube;

namespace YTMusicApi.Orchestrator.Playlist
{
    public class PlaylistOrchestrator : IPlaylistOrchestrator
    {
        private readonly IYouTubeRepository _youTubeRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IPlaylistTrackOrchestrator _playlitTrackOrchestrator;
        private readonly ITrackRepository _trackRepository;

        public PlaylistOrchestrator(
            IYouTubeRepository youTubeRepository,
            IPlaylistRepository playlistRepository,
            IPlaylistTrackOrchestrator playlistTrackOrchestrator,
            ITrackRepository trackRepository)
        {
            _youTubeRepository = youTubeRepository;
            _playlistRepository = playlistRepository;
            _playlitTrackOrchestrator = playlistTrackOrchestrator;
            _trackRepository = trackRepository;
        }

        public async Task<PlaylistDto> PostPlaylistAsync(string playlistId)
        {
            var existingPlaylist = await GetByIdPlaylistAsync(playlistId);
            if (existingPlaylist != null)
                return existingPlaylist;

            var youTubePlaylist = await _youTubeRepository.GetPlaylistAsync(playlistId);
            if (youTubePlaylist == null)
                throw new ArgumentException("Playlist not found on YouTube.");

            var savedPlaylist = await _playlistRepository.PostPlaylistAsync(youTubePlaylist);

            var youTubeTrackIds = await _youTubeRepository.GetPlaylistVideoIdsAsync(playlistId);

            var dbTrackIds = await _playlitTrackOrchestrator.GetTracksForPlaylistAsync(playlistId);

            var toAdd = youTubeTrackIds.Except(dbTrackIds).ToList();
            var toRemove = dbTrackIds.Except(youTubeTrackIds).ToList();

            if (toAdd.Any())
            {
                var missingTrackDtos = await _youTubeRepository.GetTracksAsync(toAdd);
                foreach (var missingTrackDto in missingTrackDtos)
                    await _trackRepository.PostTrackAsync(missingTrackDto);

                foreach (var trackId in toAdd)
                    await _playlitTrackOrchestrator.PostTrackToPlaylistAsync(playlistId, trackId);

                foreach (var trackId in toRemove)
                    await _playlitTrackOrchestrator.DeleteTrackFromPlaylistAsync(playlistId, trackId);
            }
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
            var playlistDto = await _youTubeRepository.GetPlaylistAsync(playlistId);
            if (playlistDto == null)
            {
                throw new ArgumentNullException("Playlist not found in YouTube Music.");
            }
            return await _playlistRepository.UpdatePlaylistAsync(playlistDto);
        }
    }
}
