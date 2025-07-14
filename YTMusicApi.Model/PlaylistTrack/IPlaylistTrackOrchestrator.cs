using YTMusicApi.Model.Track;

namespace YTMusicApi.Model.PlaylistTrack
{
    public interface IPlaylistTrackOrchestrator
    {
        Task<PlaylistTrackDto> PostTrackToPlaylistAsync(string playlistId, string trackId);
        Task<PlaylistTrackDto> DeleteTrackFromPlaylistAsync(string playlistId, string trackId);
        Task<List<TrackDto>> GetTracksForPlaylistAsync(string playlistId);
        Task UpdateTracksFromPlaylistAsync(string playlistId);
        Task<List<TrackDto>> UpdateTracksDataFromPlaylist(string playlistId);
    }
}
