using AutoMapper;
using YTMusicApi.Model.Track;

namespace YTMusicApi.Data.Track
{
    public class TrackDaoProfile : Profile
    {
        public TrackDaoProfile()
        {
            CreateMap<TrackDao, TrackDto>().ReverseMap();
        } 
    }
}
