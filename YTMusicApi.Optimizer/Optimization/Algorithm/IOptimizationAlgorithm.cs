using YTMusicApi.Optimizer.Optimization.Models;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Optimizer.Optimization.Algorithm
{
    public interface IOptimizationAlgorithm
    {
        OptimizationAlgorithmType AlgorithmType { get; }
        AlgorithmResult Optimize(OptimizationGraph graph, TimeSpan timeLimit, int maxTracks, string? startTrackId = null);
    }
}
