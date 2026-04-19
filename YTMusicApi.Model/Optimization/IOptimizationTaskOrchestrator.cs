using YTMusicApi.Shared.MessageBroker;

namespace YTMusicApi.Model.Optimization
{
    public interface IOptimizationTaskOrchestrator
    {
        Task HandleOptimizationResultAsync(OptimizationCompletedEvent @event);
        Task<OptimizationStatusResponseDto> GetOptimizationStatusAsync(Guid taskId, Guid userId);
    }
}