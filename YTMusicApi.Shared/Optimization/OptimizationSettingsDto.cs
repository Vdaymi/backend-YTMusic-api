namespace YTMusicApi.Shared.Optimization
{
    public class OptimizationSettingsDto
    {
        public List<TrackOptimizationDto> SourceTracks { get; set; } = new();
        public string? StartTrackId { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public int MaxTracks { get; set; }
        public OptimizationAlgorithmType Algorithm { get; set; } = OptimizationAlgorithmType.Greedy;
        public double GenreWeight { get; set; } = 0.5;
        public double YearWeight { get; set; } = 0.5;

    }
}