namespace YTMusicApi.Model.User
{
    public interface IUserOrchestrator
    {
        Task RegisterAsync(string username, string email, string password);
        Task<string> LoginAsync(string username, string password);
    }
}
