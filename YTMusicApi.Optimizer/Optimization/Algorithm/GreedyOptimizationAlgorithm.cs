using YTMusicApi.Optimizer.Optimization.Models;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Optimizer.Optimization.Algorithm
{
    public class GreedyOptimizationAlgorithm : IOptimizationAlgorithm
    {
        public OptimizationAlgorithmType AlgorithmType => OptimizationAlgorithmType.Greedy;

        private readonly Random _random = new Random();
        private const double Lambda = 5.0; // штраф за різкий перехід
        private const double MaxTimeOverrunSeconds = 30.0;

        public AlgorithmResult Optimize(
            OptimizationGraph graph,
            TimeSpan timeLimit,
            int maxTracks,
            string? startTrackId = null)
        {
            if (graph.Tracks == null || !graph.Tracks.Any())
                return new AlgorithmResult();

            var resultIds = new List<string>();
            var candidates = new List<TrackOptimizationDto>(graph.Tracks);

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
            double totalScore = graph.TrackScores[currentTrack.TrackId];
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

                    double w_j = graph.TrackScores[candidate.TrackId];
                    double moveCost = graph.TransitionCosts[(currentTrack.TrackId, candidate.TrackId)];

                    double f = w_j / (1.0 + Lambda * moveCost);

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
                
                double transitionCost = graph.TransitionCosts[(currentTrack.TrackId, bestNode.TrackId)];
                totalScore += graph.TrackScores[bestNode.TrackId] - (transitionCost * Lambda);

                currentTrack = bestNode;
                candidates.Remove(bestNode);
            }

            return new AlgorithmResult { TrackIds = resultIds, TotalScore = totalScore };
        }
    }
}