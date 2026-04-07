using System.Diagnostics;
using YTMusicApi.Optimizer.Optimization.Algorithm;
using YTMusicApi.Optimizer.Optimization.Models;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Optimizer.Optimization
{
    public class OptimizationOrchestrator : IOptimizationOrchestrator
    {
        private readonly IReadOnlyDictionary<OptimizationAlgorithmType, IOptimizationAlgorithm> _algorithms;
        private readonly IScoreEvaluator _scoreEvaluator;

        public OptimizationOrchestrator(IEnumerable<IOptimizationAlgorithm> algorithms, IScoreEvaluator scoreEvaluator)
        {
            _algorithms = algorithms.ToDictionary(a => a.AlgorithmType);
            _scoreEvaluator = scoreEvaluator;
        }

        public Task<OptimizationResponse> OptimizeAsync(OptimizationSettingsDto request)
        {
            if (request.SourceTracks == null || !request.SourceTracks.Any())
            {
                return Task.FromResult(new OptimizationResponse { Success = false, ErrorMessage = "Source tracks list is empty." });
            }

            if (request.TimeLimit.TotalMinutes < 1)
            {
                return Task.FromResult(new OptimizationResponse { Success = false, ErrorMessage = "Time limit must be at least 1 minute." });
            }

            if (!_algorithms.TryGetValue(request.Algorithm, out var algorithm))
            {
                return Task.FromResult(new OptimizationResponse { Success = false, ErrorMessage = $"Algorithm '{request.Algorithm}' is not supported." });
            }

            try
            {
                var trackScores = new Dictionary<string, double>();
                var transitionCosts = new Dictionary<(string, string), double>();

                foreach (var track in request.SourceTracks)
                {
                    trackScores[track.TrackId] = _scoreEvaluator.CalculateTrackScore(track);
                }

                for (int i = 0; i < request.SourceTracks.Count; i++)
                {
                    for (int j = 0; j < request.SourceTracks.Count; j++)
                    {
                        if (i == j) continue;
                        var trackA = request.SourceTracks[i];
                        var trackB = request.SourceTracks[j];
                        
                        transitionCosts[(trackA.TrackId, trackB.TrackId)] = 
                            _scoreEvaluator.CalculateTransitionCost(trackA, trackB, request.GenreWeight, request.YearWeight);
                    }
                }

                var graph = new OptimizationGraph
                {
                    Tracks = request.SourceTracks,
                    TrackScores = trackScores,
                    TransitionCosts = transitionCosts
                };
                
                var stopwatch = Stopwatch.StartNew();
                var result = algorithm.Optimize(graph, request.TimeLimit, request.MaxTracks, request.StartTrackId);
                stopwatch.Stop();

                if (!result.TrackIds.Any())
                {
                    return Task.FromResult(new OptimizationResponse { Success = false, ErrorMessage = "Algorithm failed to find a valid playlist path." });
                }

                return Task.FromResult(new OptimizationResponse { 
                    Success = true, 
                    OrderedTrackIds = result.TrackIds,
                    TotalScore = result.TotalScore,
                    ExecutionTime = stopwatch.Elapsed
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OptimizationResponse { Success = false, ErrorMessage = $"Internal Optimizer Error: {ex.Message}" });
            }
        }
    }
}