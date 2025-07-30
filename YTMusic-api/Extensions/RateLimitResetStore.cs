using System;
using System.Collections.Concurrent;

namespace YTMusicApi.Extensions
{
    public static class RateLimitResetStore
    {
        public static ConcurrentDictionary<string, DateTime> Data { get; }
            = new ConcurrentDictionary<string, DateTime>();
    }
}
