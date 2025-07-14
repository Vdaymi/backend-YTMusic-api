using AutoMapper;
using YTMusicApi.Model.UserPlaylist;

namespace YTMusicApi.Data.UserPlaylist
{
    public class UserPlaylistDaoProfile : Profile
    {
        public UserPlaylistDaoProfile()
        {
            CreateMap<UserPlaylistDao, UserPlaylistDto>().ReverseMap();
        }
    }
}
