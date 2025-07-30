namespace YTMusicApi.Model.Playlist
{
    public interface IPlaylistRepository
    {
        Task<PlaylistDto> PostPlaylistAsync(PlaylistDto playlistDto);
        Task<PlaylistDto> GetByIdPlaylistAsync(string playlistId);
        Task<List<PlaylistDto>> GetPlaylistsByIdsAsync(List<string> playlistIds);
        Task<PlaylistDto> UpdatePlaylistAsync(PlaylistDto playlistDto);
    }
}
