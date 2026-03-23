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
        [Column("user_name"), Required, StringLength(128)]
        public string UserName { get; set; }
        [Column("password_hash"), Required, MaxLength(255)]
        public string PasswordHash { get; set; }
        [Column("email"), Required, MaxLength(255)]
        public string Email { get; set; }
        [Column("is_email_verified")]
        public bool IsEmailVerified { get; set; } = false;
        [Column("email_verification_token")]
        public string? EmailVerificationToken { get; set; }
        [Column("email_verification_token_expires")]
        public DateTime? EmailVerificationTokenExpires { get; set; }

        public ICollection<UserPlaylistDao> UserPlaylists { get; set; } = new List<UserPlaylistDao>();
    }
}
