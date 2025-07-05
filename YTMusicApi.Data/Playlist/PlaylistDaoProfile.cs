using AutoMapper;
using YTMusicApi.Model.Playlist;

namespace YTMusicApi.Data.Playlist
{
    public class PlaylistDaoProfile : Profile
    {
        public PlaylistDaoProfile()
        {
            CreateMap<PlaylistDao, PlaylistDto>().ReverseMap();
        }
    }
}
