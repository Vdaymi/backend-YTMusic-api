using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Model.Integration
{
    public interface IOptimizerClient
    {
        Task<OptimizationResponse> OptimizePlaylistAsync(OptimizationSettingsDto request);
    }
}
