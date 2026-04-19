using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using YTMusicApi.Optimizer.Optimization;
using YTMusicApi.Shared.MessageBroker;
using YTMusicApi.Shared.Messaging;

namespace YTMusicApi.Optimizer.MessageBroker
{
    public class OptimizationCommandConsumer : BackgroundService
    {
        private readonly ConnectionFactory _factory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OptimizationCommandConsumer> _logger;
        private IConnection? _connection;
        private IChannel? _channel;

        public OptimizationCommandConsumer(
            IOptions<RabbitMqSettings> options, 
            IServiceScopeFactory scopeFactory,
            ILogger<OptimizationCommandConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            var settings = options.Value;
            
            _factory = new ConnectionFactory
            {
                HostName = settings.HostName,
                UserName = settings.UserName,
                Password = settings.Password
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _connection = await _factory.CreateConnectionAsync(stoppingToken);
                    _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "RabbitMQ connection failed in Optimizer. Retrying in 5 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            if (stoppingToken.IsCancellationRequested) return;
            
            await _channel.ExchangeDeclareAsync(MessagingConstants.OptimizationExchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            
            var queueName = "optimizer_commands_queue";
            await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            
            await _channel.QueueBindAsync(queueName, MessagingConstants.OptimizationExchange, MessagingConstants.OptimizeCommandRoutingKey, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var command = JsonSerializer.Deserialize<OptimizePlaylistCommand>(message);
                    if (command != null)
                    {
                        _logger.LogInformation("Received optimization command for TaskId: {TaskId}", command.TaskId);
                        
                        using var scope = _scopeFactory.CreateScope();
                        var orchestrator = scope.ServiceProvider.GetRequiredService<IOptimizationOrchestrator>();
                        
                        var result = await orchestrator.OptimizeAsync(command.Settings);

                        var completedEvent = new OptimizationCompletedEvent
                        {
                            TaskId = command.TaskId,
                            Success = result.Success,
                            OrderedTrackIds = result.OrderedTrackIds,
                            TotalScore = result.TotalScore,
                            ExecutionTime = result.ExecutionTime,
                            ErrorMessage = result.ErrorMessage
                        };

                        var eventJson = JsonSerializer.Serialize(completedEvent);
                        var eventBody = Encoding.UTF8.GetBytes(eventJson);

                         await _channel.BasicPublishAsync(
                            exchange: MessagingConstants.OptimizationExchange, 
                            routingKey: MessagingConstants.OptimizationResultRoutingKey, 
                            body: eventBody);
                            
                        _logger.LogInformation("Published result for TaskId: {TaskId}", command.TaskId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing optimization command.");
                }
                finally
                {
                   await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };

             await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        }

        public override async void Dispose()
        {
            if (_channel is not null) await _channel.DisposeAsync();
            if (_connection is not null) await _connection.DisposeAsync();
            base.Dispose();
        }
    }
}