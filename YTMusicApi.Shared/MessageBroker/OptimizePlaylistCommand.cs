using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Shared.MessageBroker
{
    public class OptimizePlaylistCommand
    {
        public Guid TaskId { get; set; }
        public OptimizationSettingsDto Settings { get; set; } = new();
    }
}