using YTMusicApi.Optimizer.Optimization.Models;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Optimizer.Optimization.Algorithm
{
    public class AntColonyOptimizationAlgorithm : IOptimizationAlgorithm
    {
        public OptimizationAlgorithmType AlgorithmType => OptimizationAlgorithmType.AntColony;

        private readonly Random _random = new Random();

        // Параметри алгоритму
        private const int Ants = 20; // кількість мурашок
        private const int MaxIter = 50; // максимальна кількість ітерацій
        private const double Alpha = 1.0; // вплив феромонів
        private const double Beta = 2.0; // вплив евристичної інформації
        private const double Rho = 0.1; // коефіцієнт випаровування феромону
        private const double Tau0 = 1.0; // початковий рівень феромону
        private const double Lambda = 5.0; // штраф за різкий перехід

        public AlgorithmResult Optimize(OptimizationGraph graph, TimeSpan timeLimit, int maxTracks, string? startTrackId = null)
        {
            if (graph.Tracks == null || !graph.Tracks.Any()) return new AlgorithmResult();
            
            var tracksDict = graph.Tracks.ToDictionary(t => t.TrackId);
            
            var pheromones = new Dictionary<(string, string), double>();

            List<string> bestPath = new List<string>();
            
            double bestScore = double.NegativeInfinity;

            for (int iter = 0; iter < MaxIter; iter++)
            {
                var antPaths = new List<List<string>>();
                var antScores = new List<double>();

                for (int ant = 0; ant < Ants; ant++)
                {
                    var currentPath = new List<string>();
                    var visited = new HashSet<string>();

                    string current = (startTrackId != null && tracksDict.ContainsKey(startTrackId))
                        ? startTrackId
                        : graph.Tracks[_random.Next(graph.Tracks.Count)].TrackId;

                    currentPath.Add(current);
                    visited.Add(current);
                    
                    TimeSpan remainingTime = timeLimit - tracksDict[current].Duration;

                    while (visited.Count < graph.Tracks.Count && currentPath.Count < maxTracks)
                    {
                        var allowed = graph.Tracks
                            .Where(t => !visited.Contains(t.TrackId) && t.Duration <= remainingTime)
                            .ToList();

                        if (!allowed.Any()) break;

                        double sumProb = 0;
                        var probs = new Dictionary<string, double>();

                        foreach (var nextTrack in allowed)
                        {
                            double k_ij = graph.TransitionCosts[(current, nextTrack.TrackId)];
                            
                            double eta = graph.TrackScores[nextTrack.TrackId] / (1.0 + Lambda * k_ij);
                            
                            double tau = pheromones.GetValueOrDefault((current, nextTrack.TrackId), Tau0);

                            double p = Math.Pow(tau, Alpha) * Math.Pow(eta, Beta);
                            probs[nextTrack.TrackId] = p;
                            sumProb += p;
                        }

                        string? nextObj = null;
                        if (sumProb > 0)
                        {
                            double rand = _random.NextDouble() * sumProb;
                            double cumulative = 0;
                            foreach (var kvp in probs)
                            {
                                cumulative += kvp.Value;
                                if (cumulative >= rand)
                                {
                                    nextObj = kvp.Key;
                                    break;
                                }
                            }
                        }

                        if (nextObj == null) nextObj = allowed.First().TrackId;

                        currentPath.Add(nextObj);
                        visited.Add(nextObj);
                        
                        remainingTime -= tracksDict[nextObj].Duration;
                        
                        current = nextObj;
                    }

                    double pathScore = 0;
                    for (int i = 0; i < currentPath.Count; i++)
                    {
                        pathScore += graph.TrackScores[currentPath[i]];
                        
                        if (i > 0)
                        {
                            double cost = graph.TransitionCosts[(currentPath[i - 1], currentPath[i])];
                            pathScore -= Lambda * cost;
                        }
                    }

                    antPaths.Add(currentPath);
                    antScores.Add(pathScore);

                    if (pathScore > bestScore)
                    {
                        bestScore = pathScore;
                        bestPath = new List<string>(currentPath);
                    }
                }

                foreach (var key in pheromones.Keys.ToList())
                {
                    pheromones[key] *= (1.0 - Rho);
                }

                for (int a = 0; a < Ants; a++)
                {
                    var path = antPaths[a];
                    
                    double deposit = Math.Max(0.01, antScores[a]); 

                    for (int step = 0; step < path.Count - 1; step++)
                    {
                        var edge = (path[step], path[step + 1]);
                        pheromones[edge] = pheromones.GetValueOrDefault(edge, Tau0) + deposit;
                    }
                }
            }
            return new AlgorithmResult { TrackIds = bestPath, TotalScore = bestScore };
        }
    }
}