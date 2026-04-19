using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YTMusicApi.Data;
using YTMusicApi.Model.MessageBroker;

namespace YTMusicApi.Platform.MessageBroker
{
    public class OutboxMessageRelayService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxMessageRelayService> _logger;

        public OutboxMessageRelayService(IServiceScopeFactory scopeFactory, ILogger<OutboxMessageRelayService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqlDbContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

            var messages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOn == null)
                .OrderBy(m => m.OccurredOn)
                .Take(20)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                try
                {
                    await publisher.PublishAsync(message.Payload, message.RoutingKey, message.Exchange);
                    message.ProcessedOn = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);
                }
            }

            if (messages.Any()) await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}