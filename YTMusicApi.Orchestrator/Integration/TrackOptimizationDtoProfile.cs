using AutoMapper;
using YTMusicApi.Model.Track;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Orchestrator.Integration
{
    public class TrackOptimizationDtoProfile : Profile
    {
        public TrackOptimizationDtoProfile()
        {
            CreateMap<TrackDto, TrackOptimizationDto>().ReverseMap();
        }
    }
}
