using AutoMapper;
using YTMusicApi.Model.PlaylistTrack;

namespace YTMusicApi.Data.PlaylistTrack
{
    public class PlaylistTrackDaoProfile : Profile
    {
        public PlaylistTrackDaoProfile()
        {
            CreateMap<PlaylistTrackDao, PlaylistTrackDto>().ReverseMap();
        }
    }
}
