using System.ComponentModel.DataAnnotations;

namespace YTMusicApi.User.Contracts
{
    public class LoginUser
    {
        [Required, MaxLength(255), EmailAddress]
        public string Email { get; set; }
        [Required, Length(8, 100)]
        public string Password { get; set; }
    }
}
