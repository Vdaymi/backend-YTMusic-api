using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.Track;

namespace YTMusicApi.Model.YouTube
{
    public interface IYouTubeRepository
    {
        Task<TrackDto> GetTrackAsync(string urlTrack);
        Task<List<TrackDto>> GetTracksAsync(List<string> trackIds);
        Task<PlaylistDto> GetPlaylistAsync(string playlistId);
        Task<List<string>> GetPlaylistVideoIdsAsync(string playlistId);
    }
}
