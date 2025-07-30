using Microsoft.AspNetCore.RateLimiting;
using YTMusicApi.Extensions;

namespace YTMusicApi.Middleware
{
    public class RateLimitResetMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TimeSpan _updatePeriod;

        public RateLimitResetMiddleware(RequestDelegate next)
        {
            _next = next;
            _updatePeriod = TimeSpan.FromMinutes(RateLimitingExtensions.updateReplenishmentPeriodFromMinutes);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var rateLimitAttr = endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>();

            if (rateLimitAttr == null)
            {
                await _next(context);
                return;
            }

            var policyName = rateLimitAttr?.PolicyName ?? "default";

            var userId = context.User?.FindFirst("sub")?.Value
                         ?? context.Connection.RemoteIpAddress?.ToString()
                         ?? "anon";

            var key = $"{userId}:{policyName}";

            RateLimitResetStore.Data.AddOrUpdate(key,
                _ => DateTime.UtcNow + _updatePeriod,
                (_, old) =>                                         
                {
                    return old < DateTime.UtcNow
                        ? DateTime.UtcNow + _updatePeriod
                        : old;
                } 
            );

            if (RateLimitResetStore.Data.TryGetValue(key, out var reset))
            {
                var seconds = (int)(reset - DateTime.UtcNow).TotalSeconds;
                if (seconds > 0)
                    context.Response.Headers["X-RateLimit-Reset"] = seconds.ToString();
            }

            await _next(context);
        }
    }
}
