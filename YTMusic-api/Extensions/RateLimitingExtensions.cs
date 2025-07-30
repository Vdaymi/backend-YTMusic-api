using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.RateLimiting;

namespace YTMusicApi.Extensions
{
    public static class RateLimitingExtensions
    {
        public static readonly int updateReplenishmentPeriodFromMinutes = 1;
        public static IServiceCollection AddPerUserRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, ct) =>
                {
                    var rateLimitAttr = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>();

                    var policyName = rateLimitAttr?.PolicyName ?? "default";

                    var userId = context.HttpContext.User?.FindFirst("sub")?.Value
                                 ?? context.HttpContext.Connection.RemoteIpAddress?.ToString()
                                 ?? "anon";

                    var key = $"{userId}:{policyName}";
                    if (RateLimitResetStore.Data.TryGetValue(key, out var reset))
                    {
                        var seconds = (int)(reset - DateTime.UtcNow).TotalSeconds;
                        if (seconds > 0)
                            context.HttpContext.Response.Headers["X-RateLimit-Reset"] = seconds.ToString();
                    }

                  
                    context.HttpContext.Response.ContentType = "application/json";

                    await context.HttpContext.Response.WriteAsync(
                        "{\"error\":\"Rate limit exceeded. Try again later.\"}", ct);
                };

                options.AddPolicy("PerUserUpdatePlaylistPolicy", context =>
                {

                    var policyName = "PerUserUpdatePlaylistPolicy";
                    var userId = context.User?.FindFirst("sub")?.Value
                              ?? context.Connection.RemoteIpAddress?.ToString()
                              ?? "anon";

                    var key = $"{userId}:{policyName}";

                    return RateLimitPartition.GetTokenBucketLimiter(key, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 1,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(updateReplenishmentPeriodFromMinutes),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
                });

                options.AddPolicy("PerUserUpdateTrackPolicy", context =>
                {
                    var policyName = "PerUserUpdateTrackPolicy";
                    var userId = context.User?.FindFirst("sub")?.Value
                              ?? context.Connection.RemoteIpAddress?.ToString()
                              ?? "anon";

                    var key = $"{userId}:{policyName}";

                    return RateLimitPartition.GetTokenBucketLimiter(key, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 1,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(updateReplenishmentPeriodFromMinutes),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
                });
            });

            return services;
        }

        public static IApplicationBuilder UsePerUserRateLimiting(this IApplicationBuilder app)
        {
            app.UseRateLimiter();
            return app;
        }
    }
}