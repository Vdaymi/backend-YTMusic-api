using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Model.Playlist
{
    public class PlaylistSettingDto
    {
        public string PlaylistId { get; set; }
        public TimeSpan TargetDuration { get; set; }
        public OptimizationAlgorithmType Algorithm { get; set; }
        public double GenreWeight { get; set; }
    }
}
