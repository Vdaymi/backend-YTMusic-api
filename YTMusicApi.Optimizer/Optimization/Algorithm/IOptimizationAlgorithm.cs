using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Optimizer.Optimization.Algorithm
{
    public interface IOptimizationAlgorithm
    {
        List<string> Optimize(List<TrackOptimizationDto> sourceTracks, TimeSpan timeLimit, int maxTracks, double wGenre, double wYear, string? startTrackId = null);
    }
}
