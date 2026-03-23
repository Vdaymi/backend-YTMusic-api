using System.ComponentModel.DataAnnotations;

namespace YTMusicApi.User.Contracts
{
    public class ResendVerificationRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
    }
}