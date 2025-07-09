using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YTMusicApi.Data.Playlist;
using YTMusicApi.Data.Track;

namespace YTMusicApi.Data.PlaylistTrack
{
    [Table("playlist_tracks")]
    public class PlaylistTrackDao
    {
        [Key, Column("playlist_id", Order = 0)]
        public string PlaylistId { get; set; }

        [Key, Column("track_id", Order = 1)]
        public string TrackId { get; set; }

        public PlaylistDao Playlist { get; set; }
        public TrackDao Track { get; set; }
    }
}
