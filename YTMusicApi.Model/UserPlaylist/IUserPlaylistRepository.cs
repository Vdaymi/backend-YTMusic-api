namespace YTMusicApi.Model.UserPlaylist
{
    public interface IUserPlaylistRepository
    {
        Task PostPlaylistToUserAsync(UserPlaylistDto userPlaylistDto);
        Task<UserPlaylistDto> DeletePlaylistFromUserAsync(UserPlaylistDto userPlaylistDto);
        Task<List<string>> GetPlaylistIdsByUserAsync(Guid userId);
    }
}
