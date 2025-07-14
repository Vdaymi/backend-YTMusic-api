using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.PlaylistTrack;
using YTMusicApi.Model.UserPlaylist;
using YTMusicApi.Model.YouTube;

namespace YTMusicApi.Orchestrator.Playlist
{
    public class PlaylistOrchestrator : IPlaylistOrchestrator
    {
        private readonly IYouTubeRepository _youTubeRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IPlaylistTrackOrchestrator _playlitTrackOrchestrator;
        private readonly IUserPlaylistOrchestrator _userPlaylistOrchestrator;

        public PlaylistOrchestrator(
            IYouTubeRepository youTubeRepository,
            IPlaylistRepository playlistRepository,
            IPlaylistTrackOrchestrator playlistTrackOrchestrator,
            IUserPlaylistOrchestrator userPlaylistOrchestrator)
        {
            _youTubeRepository = youTubeRepository;
            _playlistRepository = playlistRepository;
            _playlitTrackOrchestrator = playlistTrackOrchestrator;
            _userPlaylistOrchestrator = userPlaylistOrchestrator;
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

            await _playlitTrackOrchestrator.UpdateTracksFromPlaylistAsync(youTubePlaylist.PlaylistId);

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
            await GetByIdPlaylistAsync(playlistId);

            var playlistDto = await _youTubeRepository.GetPlaylistAsync(playlistId);
            if (playlistDto == null)
            {
                throw new ArgumentNullException("Playlist not found on YouTube Music.");
            }
            var updatedPlaylist = await _playlistRepository.UpdatePlaylistAsync(playlistDto);
            
            await _playlitTrackOrchestrator.UpdateTracksFromPlaylistAsync(playlistDto.PlaylistId);
           
            return updatedPlaylist;
        }
    }
}
