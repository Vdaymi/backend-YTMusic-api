using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Playlist.Contracts
{
    public class OptimizationRequest
    {
        [Required, Range(typeof(TimeSpan), "00:01:00", "10:00:00")]
        public TimeSpan TimeLimit { get; set; }
        [Required, Range(1, 1000)]
        public int MaxTracks { get; set; }
        public OptimizationAlgorithmType Algorithm { get; set; }
        [Required, Range(0, 1)]
        public double GenreWeight { get; set; }
        public string? StartTrackId { get; set; }
    }
}