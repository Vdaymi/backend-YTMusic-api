using AutoMapper;
using YTMusicApi.Model.User;

namespace YTMusicApi.Data.User
{
    public class UserDaoProfile : Profile
    {
        public UserDaoProfile()
        {
            CreateMap<UserDao, UserDto>().ReverseMap();
        }
    }
}
