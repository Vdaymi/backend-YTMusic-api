using YTMusicApi.Optimizer.Optimization.Algorithm;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Optimizer.Optimization
{
    public class OptimizationOrchestrator : IOptimizationOrchestrator
    {
        private readonly IOptimizationAlgorithm _algorithm;

        public OptimizationOrchestrator(IOptimizationAlgorithm algorithm)
        {
            _algorithm = algorithm;
        }

        public Task<OptimizationResponse> OptimizeAsync(OptimizationSettingsDto request)
        {
            if (request.SourceTracks == null || !request.SourceTracks.Any())
            {
                return Task.FromResult(new OptimizationResponse
                {
                    Success = false,
                    ErrorMessage = "Source tracks list is empty."
                });
            }

            if (request.TimeLimit.TotalMinutes < 1)
            {
                return Task.FromResult(new OptimizationResponse
                {
                    Success = false,
                    ErrorMessage = "Time limit must be at least 1 minute."
                });
            }

            try
            {
                // 2. Вибір стратегії (на майбутнє)
                // Поки що ми ігноруємо request.Algorithm, бо маємо тільки одну реалізацію.
                // Але тут має бути switch (request.Algorithm) { ... }

                var resultIds = _algorithm.Optimize(request.SourceTracks, request.TimeLimit, request.MaxTracks, request.GenreWeight, request.YearWeight, request.StartTrackId);

                if (!resultIds.Any())
                {
                    return Task.FromResult(new OptimizationResponse
                    {
                        Success = false,
                        ErrorMessage = "Algorithm failed to find a valid playlist path."
                    });
                }

                return Task.FromResult(new OptimizationResponse
                {
                    Success = true,
                    OrderedTrackIds = resultIds
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OptimizationResponse
                {
                    Success = false,
                    ErrorMessage = $"Internal Optimizer Error: {ex.Message}"
                });
            }
        }
    }
}