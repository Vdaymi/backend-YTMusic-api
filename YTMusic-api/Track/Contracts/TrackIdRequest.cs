using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace YTMusicApi.Track.Contracts
{
    public class TrackIdRequest
    {
        [FromRoute(Name = "trackId")]
        [Required, Length(11,11)]
        public string TrackId { get; set; }
    }
}
