using System.Collections.Generic;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Optimizer.Optimization.Models
{
    public class OptimizationGraph
    {
        public IReadOnlyList<TrackOptimizationDto> Tracks { get; set; } = new List<TrackOptimizationDto>();
        
        public IReadOnlyDictionary<string, double> TrackScores { get; set; } = new Dictionary<string, double>();
        
        public IReadOnlyDictionary<(string, string), double> TransitionCosts { get; set; } = new Dictionary<(string, string), double>();
    }
}