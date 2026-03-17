using AutoMapper;
using YTMusicApi.Model.Playlist;

namespace YTMusicApi.Data.Playlist
{
    public class PlaylistSettingDaoProfile : Profile
    {
        public PlaylistSettingDaoProfile()
        {
            CreateMap<PlaylistSettingDao, PlaylistSettingDto>().ReverseMap();
        }
    }
}
