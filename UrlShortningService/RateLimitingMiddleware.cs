using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UrlShortningService.Services.Interfaces;
using System;

namespace UrlShortningService.Services.Implementation
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider; // Add IServiceProvider
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IServiceProvider serviceProvider, int maxRequests, TimeSpan timeWindow)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _maxRequests = maxRequests;
            _timeWindow = timeWindow;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (ipAddress == null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Unable to get IP address.");
                return;
            }

            // Create a scope to resolve scoped services like ICacheService
            using (var scope = _serviceProvider.CreateScope())
            {
                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

                var key = $"requestCount:{ipAddress}";
                var currentTime = DateTime.UtcNow;

                // Get the current request count from cache
                var currentRequestCount = await cacheService.GetAsync(key);
                int requestCount = 0;

                if (!string.IsNullOrEmpty(currentRequestCount))
                {
                    requestCount = int.Parse(currentRequestCount);
                }

                // Check if rate limit is exceeded
                if (requestCount >= _maxRequests)
                {
                    _logger.LogWarning("Rate limit exceeded for IP: {IpAddress}", ipAddress);
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                    return;
                }

                // Increment request count in cache (with expiration)
                requestCount++;
                await cacheService.SetAsync(key, requestCount.ToString(), _timeWindow);

                // Proceed with the request pipeline
                await _next(context);
            }
        }
    }
}
