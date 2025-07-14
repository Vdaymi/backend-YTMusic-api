using YTMusicApi.Model.Track;

namespace YTMusicApi.Model.PlaylistTrack
{
    public interface IPlaylistTrackRepository
    {
        Task<PlaylistTrackDto> PostTrackToPlaylistAsync(PlaylistTrackDto playlistTrackDto);
        Task<PlaylistTrackDto> DeleteTrackFromPlaylistAsync(PlaylistTrackDto playlistTrackDto);
        Task<List<string>> GetTrackIdsByPlaylistAsync(string playlistId);
        Task<List<TrackDto>> GetTracksByPlaylistAsync(string playlistId);
    }
}
