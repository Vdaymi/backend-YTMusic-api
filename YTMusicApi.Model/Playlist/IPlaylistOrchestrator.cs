namespace YTMusicApi.Model.Playlist
{
    public interface IPlaylistOrchestrator
    {
        Task<PlaylistDto> PostPlaylistAsync(string playlistId, Guid userId);
        Task<PlaylistDto> GetByIdPlaylistAsync(string playlistId);
        Task<PlaylistDto> UpdatePlaylistAsync(string playlistId);
    }
}
