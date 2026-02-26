using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Playlist.Contracts
{
    public class OptimizationRequest
    {
        [FromQuery(Name = "timeLimit")]
        [Required, Range(typeof(TimeSpan), "00:01:00", "10:00:00")]
        public TimeSpan TimeLimit { get; set; }
        [FromQuery(Name = "maxTracks")]
        [Required, Range(1, 1000)]
        public int MaxTracks { get; set; }
        [FromQuery(Name = "algorithm")]
        public OptimizationAlgorithmType Algorithm { get; set; }
        [FromQuery(Name = "genreWeight")]
        [Required, Range(0, 1)]
        public double GenreWeight { get; set; }
        [FromQuery(Name = "startTrackId")]
        public string? StartTrackId { get; set; }
    }
}