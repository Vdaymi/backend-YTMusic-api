namespace YTMusicApi.Model.User
{
    public interface IUserRepository
    {
        Task AddUserAsync(UserDto userDto);
        Task<UserDto> GetByEmailAsync(string email);
        Task<UserDto> GetByVerificationTokenAsync(string token);
        Task UpdateUserAsync(UserDto userDto);
    }
}
