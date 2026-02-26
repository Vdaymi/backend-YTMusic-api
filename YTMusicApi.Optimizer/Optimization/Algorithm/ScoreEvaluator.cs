using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Optimizer.Optimization.Algorithm
{
    public class ScoreEvaluator : IScoreEvaluator
    {
        private const double MaxYearDiff = 50.0;

        public double CalculateTransitionCost(TrackOptimizationDto firstTrack, TrackOptimizationDto secondTrack, double genreWeight, double yearWeight)
        {
            double genreCost = CalculateGenreDistance(firstTrack.TopicCategories, secondTrack.TopicCategories);

            double yearCost = CalculateYearDistance(firstTrack.PublishedAt, secondTrack.PublishedAt);

            return (genreWeight * genreCost) + (yearWeight * yearCost);
        }

        public double CalculateTrackScore(TrackOptimizationDto track)
        {
            double score = 1.0;

            if (track.LikeCount.HasValue && track.LikeCount > 0)
            {
                score += Math.Log10(track.LikeCount.Value);
            }
            else if (track.ViewCount.HasValue && track.ViewCount > 0)
            {
                score += Math.Log10(track.ViewCount.Value) * 0.5;
            }

            return score;
        }

        private double CalculateGenreDistance(string? genres1, string? genres2)
        {
            if (string.IsNullOrWhiteSpace(genres1) || string.IsNullOrWhiteSpace(genres2))
                return 1.0;

            var set1 = genres1.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();
            var set2 = genres2.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();

            var intersection = set1.Intersect(set2).Count();
            var union = set1.Union(set2).Count();

            if (union == 0) return 1.0;

            double jaccardIndex = (double)intersection / union;

            return 1.0 - jaccardIndex;
        }

        private double CalculateYearDistance(DateTime? date1, DateTime? date2)
        {
            int year1 = date1?.Year ?? 2005;
            int year2 = date2?.Year ?? 2005;

            double diff = Math.Abs(year1 - year2);

            return Math.Clamp(diff / MaxYearDiff, 0.0, 1.0);
        }
    }
}