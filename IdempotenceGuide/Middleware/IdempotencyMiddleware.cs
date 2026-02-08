using IdempotenceGuide.Dto;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace IdempotenceGuide.Middleware
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;

        public IdempotencyMiddleware(RequestDelegate next, IDistributedCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only apply to POST requests
            if (context.Request.Method != HttpMethods.Post)
            {
                await _next(context);
                return;
            }

            var idempotencyKey = context.Request.Headers["Idempotency-Key"].FirstOrDefault();

            if (string.IsNullOrEmpty(idempotencyKey))
            {
                await _next(context);
                return;
            }

            // Check cache for existing response
            var cachedResponse = await _cache.GetStringAsync($"idempotency:{idempotencyKey}");

            if (cachedResponse != null)
            {
                // Return cached response
                var cached = JsonSerializer.Deserialize<CachedResponse>(cachedResponse);
                context.Response.StatusCode = cached.StatusCode;
                context.Response.ContentType = cached.ContentType;
                await context.Response.WriteAsync(cached.Body);
                return;
            }

            // Capture the response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Cache the response
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            var cacheEntry = new CachedResponse
            {
                StatusCode = context.Response.StatusCode,
                ContentType = context.Response.ContentType ?? "application/json",
                Body = responseText
            };

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            };

            await _cache.SetStringAsync(
                $"idempotency:{idempotencyKey}",
                JsonSerializer.Serialize(cacheEntry),
                cacheOptions);

            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    // Extension method for easy registration
    public static class IdempotencyMiddlewareExtensions
    {
        public static IApplicationBuilder UseIdempotency(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IdempotencyMiddleware>();
        }
    }
}
