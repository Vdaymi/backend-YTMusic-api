namespace YTMusicApi.Model.Auth
{
    public interface IEmailSender
    {
        Task SendVerificationEmailAsync(string userEmail, string verificationLink);
    }
}