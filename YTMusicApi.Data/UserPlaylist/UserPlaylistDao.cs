using System.ComponentModel.DataAnnotations.Schema;
using YTMusicApi.Data.Playlist;
using YTMusicApi.Data.User;

namespace YTMusicApi.Data.UserPlaylist
{
    [Table("user_playlists")]
    public class UserPlaylistDao
    {
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Column("playlist_id")] 
        public string PlaylistId { get; set; }
        public UserDao User { get; set; }
        public PlaylistDao Playlist { get; set; }
    }
}
