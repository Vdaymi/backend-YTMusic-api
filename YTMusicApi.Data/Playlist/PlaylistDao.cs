using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YTMusicApi.Data.PlaylistTrack;
using YTMusicApi.Data.UserPlaylist;
using YTMusicApi.Model.Playlist;

namespace YTMusicApi.Data.Playlist
{
    [Table("playlists")]
    public class PlaylistDao
    {
        [Column("playlist_id"), Key, StringLength(34)]
        public string PlaylistId { get; set; }
        [Column("title")]
        public string Title { get; set; }
        [Column("channel_title")]
        public string ChannelTitle { get; set; }
        [Column("item_count")]
        public int? ItemCount { get; set; }
        [Column("source")]
        public PlaylistSource Source { get; set; } = PlaylistSource.YouTube;
        public PlaylistSettingDao? OptimizationSetting { get; set; }
        public ICollection<PlaylistTrackDao> PlaylistTracks { get; set; } = new List<PlaylistTrackDao>();
        public ICollection<UserPlaylistDao> UserPlaylists { get; set; } = new List<UserPlaylistDao>();

    }
}
