using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using YTMusicApi.Model.MessageBroker;
using YTMusicApi.Shared.Messaging;

namespace YTMusicApi.Platform.MessageBroker
{
    public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqPublisher(IOptions<RabbitMqSettings> rabbitMqSettings)
        {
            var settings = rabbitMqSettings.Value;
            _factory = new ConnectionFactory
            {
                HostName = settings.HostName,
                UserName = settings.UserName,
                Password = settings.Password
            };
        }

        private async Task InitializeAsync()
        {
            if (_channel is not null) return;

            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            
            await _channel.ExchangeDeclareAsync(MessagingConstants.OptimizationExchange, ExchangeType.Topic, durable: true);
            
            await _channel.QueueDeclareAsync("optimizer_commands_queue", durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueBindAsync("optimizer_commands_queue", MessagingConstants.OptimizationExchange, MessagingConstants.OptimizeCommandRoutingKey);
            
            await _channel.QueueDeclareAsync("api_optimization_results_queue", durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueBindAsync("api_optimization_results_queue", MessagingConstants.OptimizationExchange, MessagingConstants.OptimizationResultRoutingKey);
        }

        public async Task PublishAsync<T>(T message, string routingKey, string exchange = "")
        {
            await InitializeAsync();

            var messageJson = message is string str ? str : JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageJson);

            await _channel!.BasicPublishAsync(exchange: exchange, routingKey: routingKey, body: body);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null) await _channel.DisposeAsync();
            if (_connection is not null) await _connection.DisposeAsync();
        }
    }
}