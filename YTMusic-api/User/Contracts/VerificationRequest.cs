using System.ComponentModel.DataAnnotations;

namespace YTMusicApi.User.Contracts
{
    public class VerificationRequest
    {
        [Required]
        public string Token { get; set; }
    }
}