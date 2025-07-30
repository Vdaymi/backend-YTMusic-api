using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.UserPlaylist;
using YTMusicApi.Model.YouTube;

namespace YTMusicApi.Orchestrator.UserPlaylist
{
    public class UserPlaylistOrchestrator : IUserPlaylistOrchestrator
    {
        private readonly IUserPlaylistRepository _userPlaylistRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IYouTubeRepository _youTubeRepository;

        public UserPlaylistOrchestrator(
            IUserPlaylistRepository userPlaylistRepository,
            IPlaylistRepository playlistRepository,
            IYouTubeRepository youTubeRepository) 
        {
            _userPlaylistRepository = userPlaylistRepository;
            _playlistRepository = playlistRepository;
            _youTubeRepository = youTubeRepository;
        }

        public async Task<List<PlaylistDto>> GetPlaylistsByUserAsync(Guid userId)
        {
            var playlistIds = await _userPlaylistRepository.GetPlaylistIdsByUserAsync(userId);
            return await _playlistRepository.GetPlaylistsByIdsAsync(playlistIds);
        }

        public async Task PostPlaylistToUserAsync(Guid userId, string playlistId) 
        {
            var userPlaylistDto = new UserPlaylistDto();
            userPlaylistDto.UserId = userId;
            userPlaylistDto.PlaylistId = playlistId;

            await _userPlaylistRepository.PostPlaylistToUserAsync(userPlaylistDto); 
        }

        public async Task<UserPlaylistDto> DeletePlaylistFromUserAsync(Guid userId, string playlistId)
        {
            var userPlaylistDto = new UserPlaylistDto();
            userPlaylistDto.UserId = userId;
            userPlaylistDto.PlaylistId = playlistId;
            
            return await _userPlaylistRepository.DeletePlaylistFromUserAsync(userPlaylistDto);
        }
    }
}
