using YTMusicApi.Model.Playlist;

namespace YTMusicApi.Model.Optimization
{
    public class OptimizationStatusResponseDto
    {
        public OptimizationTaskStatus Status { get; set; }
        public OptimizedPlaylistResultDto? Result { get; set; }
    }
}