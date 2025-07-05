using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.YouTube;

namespace YTMusicApi.Orchestrator.Playlist
{
    public class PlaylistOrchestrator : IPlaylistOrchestrator
    {
        private readonly IYouTubeRepository _youTubeRepository;
        private readonly IPlaylistRepository _playlistRepository;

        public PlaylistOrchestrator(
            IYouTubeRepository youTubeRepository,
            IPlaylistRepository playlistRepository)
        {
            _youTubeRepository = youTubeRepository;
            _playlistRepository = playlistRepository;
        }

        public async Task<PlaylistDto> PostPlaylistAsync(string playlistId)
        {
            var playlistDto = await _youTubeRepository.GetPlaylistAsync(playlistId);
            if (playlistDto == null)
            {
                throw new ArgumentNullException("Playlist not found in YouTobe Music.");
            }
            return await _playlistRepository.PostPlaylistAsync(playlistDto);
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
