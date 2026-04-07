using System.Collections.Generic;

namespace YTMusicApi.Optimizer.Optimization.Models
{
    public class AlgorithmResult
    {
        public List<string> TrackIds { get; set; } = new List<string>();
        public double TotalScore { get; set; }
    }
}