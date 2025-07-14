using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace YTMusicApi.Playlist.Contracts
{
    public class PlaylistIdRequest
    {
        [FromRoute(Name = "playlistId")]
        [Required, Length(34,34)]
        public string PlaylistId { get; set; }
    }
}
