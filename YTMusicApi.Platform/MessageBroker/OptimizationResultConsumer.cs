using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using YTMusicApi.Model.Optimization;
using YTMusicApi.Shared.MessageBroker;
using YTMusicApi.Shared.Messaging;

namespace YTMusicApi.Platform.MessageBroker
{
    public class OptimizationResultConsumer : BackgroundService
    {
        private readonly ConnectionFactory _factory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OptimizationResultConsumer> _logger;
        private IConnection? _connection;
        private IChannel? _channel;

        public OptimizationResultConsumer(IOptions<RabbitMqSettings> options, IServiceScopeFactory scopeFactory, ILogger<OptimizationResultConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            var settings = options.Value;
            _factory = new ConnectionFactory { HostName = settings.HostName, UserName = settings.UserName, Password = settings.Password };
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
                    _logger.LogWarning(ex, "RabbitMQ connection failed in API. Retrying in 5 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            if (stoppingToken.IsCancellationRequested) return;
            
            var queueName = "api_optimization_results_queue";
            await _channel.ExchangeDeclareAsync(MessagingConstants.OptimizationExchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(queueName, MessagingConstants.OptimizationExchange, MessagingConstants.OptimizationResultRoutingKey, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var completedEvent = JsonSerializer.Deserialize<OptimizationCompletedEvent>(message);
                    
                    if (completedEvent != null)
                    {
                        _logger.LogInformation("Received optimization result for TaskId: {TaskId}", completedEvent.TaskId);
                        
                        using var scope = _scopeFactory.CreateScope();
                        var orchestrator = scope.ServiceProvider.GetRequiredService<IOptimizationTaskOrchestrator>();
                        
                        await orchestrator.HandleOptimizationResultAsync(completedEvent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing optimization result.");
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