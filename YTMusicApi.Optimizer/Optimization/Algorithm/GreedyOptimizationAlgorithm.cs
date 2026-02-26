using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Optimizer.Optimization.Algorithm
{
    public class GreedyOptimizationAlgorithm : IOptimizationAlgorithm
    {
        private readonly IScoreEvaluator _scoreEvaluator;
        private readonly Random _random = new Random();
        private const double Penalty = 5.0;
        private const double MaxTimeOverrunSeconds = 30.0;

        public GreedyOptimizationAlgorithm(IScoreEvaluator scoreEvaluator)
        {
            _scoreEvaluator = scoreEvaluator;
        }

        public List<string> Optimize(
            List<TrackOptimizationDto> sourceTracks,
            TimeSpan timeLimit,
            int maxTracks,
            double wGenre,
            double wYear,
            string? startTrackId = null)
        {
            if (sourceTracks == null || !sourceTracks.Any())
                return new List<string>();

            var resultIds = new List<string>();

            var candidates = new List<TrackOptimizationDto>(sourceTracks);

            double currentPlaylistDuration = 0;
            double maxTimeD = timeLimit.TotalSeconds;

            TrackOptimizationDto currentTrack;

            if (!string.IsNullOrEmpty(startTrackId))
            {
                var requested = candidates.FirstOrDefault(t => t.TrackId == startTrackId);
                currentTrack = requested ?? candidates[_random.Next(candidates.Count)];
            }
            else
            {
                currentTrack = candidates[_random.Next(candidates.Count)];
            }

            resultIds.Add(currentTrack.TrackId);
            currentPlaylistDuration += currentTrack.Duration.TotalSeconds;
            candidates.Remove(currentTrack);

            while (candidates.Any() && currentPlaylistDuration < maxTimeD && resultIds.Count < maxTracks)
            {
                TrackOptimizationDto? bestNode = null;
                double bestF = double.NegativeInfinity;

                foreach (var candidate in candidates)
                {
                    if (currentPlaylistDuration + candidate.Duration.TotalSeconds > maxTimeD + MaxTimeOverrunSeconds)
                    {
                        continue;
                    }

                    double w_j = _scoreEvaluator.CalculateTrackScore(candidate);
                    double moveCost = _scoreEvaluator.CalculateTransitionCost(currentTrack, candidate, wGenre, wYear);

                    double f = w_j - (moveCost * Penalty);

                    if (f > bestF)
                    {
                        bestF = f;
                        bestNode = candidate;
                    }
                }

                if (bestNode == null)
                {
                    break;
                }

                resultIds.Add(bestNode.TrackId);
                currentPlaylistDuration += bestNode.Duration.TotalSeconds;

                currentTrack = bestNode;
                candidates.Remove(bestNode);
            }

            return resultIds;
        }
    }
}