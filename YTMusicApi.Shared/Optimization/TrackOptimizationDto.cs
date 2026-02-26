namespace YTMusicApi.Shared.Optimization
{
    public class TrackOptimizationDto
    {
        public string TrackId { get; set; } = string.Empty;
        public long? ViewCount { get; set; }
        public long? LikeCount { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? TopicCategories { get; set; }
    }
}