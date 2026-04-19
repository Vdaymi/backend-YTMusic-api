namespace YTMusicApi.Model.MessageBroker
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, string routingKey, string exchange = "");
    }
}