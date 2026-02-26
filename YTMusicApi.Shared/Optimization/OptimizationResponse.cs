namespace YTMusicApi.Shared.Optimization
{
    public class OptimizationResponse
    {
        public List<string> OrderedTrackIds { get; set; } = new();
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }
}