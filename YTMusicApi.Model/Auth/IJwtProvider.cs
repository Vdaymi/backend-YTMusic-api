using YTMusicApi.Model.User;

namespace YTMusicApi.Model.Auth
{
    public interface IJwtProvider
    {
        string GenerateToken(UserDto userDto);
    }
}
