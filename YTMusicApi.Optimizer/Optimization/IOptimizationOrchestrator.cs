using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Optimizer.Optimization
{
    public interface IOptimizationOrchestrator
    {
        Task<OptimizationResponse> OptimizeAsync(OptimizationSettingsDto request);
    }
}