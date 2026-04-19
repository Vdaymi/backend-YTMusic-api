namespace YTMusicApi.Shared.Messaging
{
    public static class MessagingConstants
    {
        public const string OptimizationExchange = "optimization.exchange";

        public const string OptimizeCommandRoutingKey = "optimization.command.optimize";
        public const string OptimizationResultRoutingKey = "optimization.event.completed";
    }
}