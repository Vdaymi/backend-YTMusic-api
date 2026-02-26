using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Optimizer.Optimization.Algorithm
{
    public interface IScoreEvaluator
    {
        double CalculateTransitionCost(TrackOptimizationDto firstTrack, TrackOptimizationDto secondTrack, double genreWeight, double yearWeight);
        double CalculateTrackScore(TrackOptimizationDto track);
    }
}