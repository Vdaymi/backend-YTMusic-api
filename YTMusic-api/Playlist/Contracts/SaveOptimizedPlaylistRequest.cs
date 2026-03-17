using System.ComponentModel.DataAnnotations;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Playlist.Contracts
{
    public class SaveOptimizedPlaylistRequest
    {
        [Required, MaxLength(255)]
        public string Title { get; set; }
        [Required, MaxLength(255)]
        public string ChannelTitle { get; set; }
        [Required]
        public List<string> TrackIds { get; set; }
        [Required]
        public TimeSpan TargetDuration { get; set; }
        [Required]
        public OptimizationAlgorithmType Algorithm { get; set; }
        [Required]
        public double GenreWeight { get; set; }
    }
}
