namespace YTMusicApi.Shared.MessageBroker
{
    public class OptimizationCompletedEvent
    {
        public Guid TaskId { get; set; }
        public List<string> OrderedTrackIds { get; set; } = new();
        public double TotalScore { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}