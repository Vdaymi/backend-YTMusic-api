using AutoMapper;
using YTMusicApi.Model.Optimization;

namespace YTMusicApi.Data.Optimization
{
    public class OptimizationTaskDaoProfile : Profile
    {
        public OptimizationTaskDaoProfile()
        {
            CreateMap<OptimizationTaskDto, OptimizationTaskDao>().ReverseMap();
        }
    }
}