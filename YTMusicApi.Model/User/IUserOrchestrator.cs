using YTMusicApi.Model.Auth;

namespace YTMusicApi.Model.User
{
    public interface IUserOrchestrator
    {
        Task RegisterAsync(string username, string email, string password);
        Task<LoginResultDto> LoginAsync(string username, string password);
        Task VerifyEmailAsync(string token);
        Task ResendVerificationEmailAsync(string email);
    }
}
