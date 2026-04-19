namespace YTMusicApi.Optimizer.MessageBroker
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; } = "localhost";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
    }
}