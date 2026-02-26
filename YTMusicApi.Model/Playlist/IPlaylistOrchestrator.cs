using YTMusicApi.Model.Track;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Model.Playlist
{
    public interface IPlaylistOrchestrator
    {
        Task<PlaylistDto> PostPlaylistAsync(string playlistId, Guid userId);
        Task<PlaylistDto> GetByIdPlaylistAsync(string playlistId);
        Task<PlaylistDto> UpdatePlaylistAsync(string playlistId);
        Task<List<TrackDto>> GetOptimizedTracksAsync(string playlistId, TimeSpan timeLimit, int? maxTracks, OptimizationAlgorithmType algorithm, double genreWeight, string? startTrackId);
    }
}
