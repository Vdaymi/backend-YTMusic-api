using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YTMusicApi.Data.PlaylistTrack;
using YTMusicApi.Data.UserPlaylist;

namespace YTMusicApi.Data.Playlist
{
    [Table("playlists")]
    public class PlaylistDao
    {
        [Column("playlist_id"), Key, Length(34, 34)]
        public string PlaylistId { get; set; }
        [Column("title")]
        public string Title { get; set; }
        [Column("channel_title")]
        public string СhannelTitle { get; set; }
        [Column("item_count")]
        public int? ItemCount { get; set; }

        public ICollection<PlaylistTrackDao> PlaylistTracks { get; set; } = new List<PlaylistTrackDao>();
        public ICollection<UserPlaylistDao> UserPlaylists { get; set; } = new List<UserPlaylistDao>();

    }
}
