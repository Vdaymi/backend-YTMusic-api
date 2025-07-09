using YTMusicApi.Model.PlaylistTrack;
using YTMusicApi.Model.Track;

namespace YTMusicApi.Model.Playlist
{
    public interface IPlaylistOrchestrator
    {
        Task<PlaylistDto> PostPlaylistAsync(string playlistId);
        Task<PlaylistDto> GetByIdPlaylistAsync(string playlistId);
        Task<PlaylistDto> UpdatePlaylistAsync(string playlistId);
    }
}
