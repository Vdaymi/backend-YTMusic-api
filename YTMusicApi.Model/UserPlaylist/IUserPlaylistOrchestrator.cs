using YTMusicApi.Model.Playlist;

namespace YTMusicApi.Model.UserPlaylist
{
    public interface IUserPlaylistOrchestrator
    {
        public Task<List<PlaylistDto>> GetPlaylistsByUserAsync(Guid userId);
        public Task PostPlaylistToUserAsync(Guid userId, string playlistId);
        public Task<UserPlaylistDto> DeletePlaylistFromUserAsync(Guid userId, string playlistId);
    }
}
