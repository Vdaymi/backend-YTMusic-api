using AutoMapper;
using YTMusicApi.Model.MessageBroker;

namespace YTMusicApi.Data.MessageBroker
{
    public class OutboxMessageDaoProfile : Profile
    {
        public OutboxMessageDaoProfile()
        {
            CreateMap<OutboxMessageDto, OutboxMessageDao>().ReverseMap();
        }
    }
}