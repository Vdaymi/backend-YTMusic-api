using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YTMusicApi.Data.UserPlaylist;

namespace YTMusicApi.Data.User
{
    [Table("users")]
    public class UserDao
    {
        [Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Column("user_name"), Required, Length(3, 128)]
        public string UserName { get; set; }
        [Column("password_hash"), Required, MaxLength(255)]
        public string PasswordHash { get; set; }
        [Column("email"), Required, MaxLength(255)]
        public string Email { get; set; }

        public ICollection<UserPlaylistDao> UserPlaylists { get; set; } = new List<UserPlaylistDao>();
    }
}
