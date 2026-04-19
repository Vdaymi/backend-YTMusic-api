namespace YTMusicApi.Model.Optimization
{
    public class OptimizationTaskDto
    {
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public string PlaylistId { get; set; } = string.Empty;
        public OptimizationTaskStatus Status { get; set; }
    }
}