using System.ComponentModel.DataAnnotations;

namespace YTMusicApi.User.Contracts
{
    public class RegisterUser
    {
        [Required, Length(3, 128), RegularExpression(@"^[a-zA-Z0-9_\.]+$")]
        public string UserName { get; set; }
        [Required, Length(8, 100), RegularExpression(@"(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+")]
        public string Password { get; set; }
        [Required, MaxLength(255), EmailAddress]
        public string Email { get; set; }
    }
}
