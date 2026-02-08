using IdempotenceGuide.Contract;
using IdempotenceGuide.Entity;
using Microsoft.Extensions.Caching.Distributed;

namespace IdempotenceGuide.Service
{
    public class MessageQueueIdempotence : IMessageProcessor
    {
        private readonly IDistributedCache _cache;
        private readonly IOrderService _orderService;
        private readonly ILogger<MessageQueueIdempotence> _logger;

        public MessageQueueIdempotence(
            IDistributedCache cache,
            IOrderService orderService,
            ILogger<MessageQueueIdempotence> logger)
        {
            _cache = cache;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task ProcessMessageAsync(OrderMessage message)
        {
            var processingKey = $"message:processed:{message.MessageId}";
            var lockKey = $"message:lock:{message.MessageId}";

            // Check if already processed
            if (await _cache.GetStringAsync(processingKey) != null)
            {
                _logger.LogInformation(
                    "Message {MessageId} already processed, skipping",
                    message.MessageId);
                return;
            }

            // Distributed lock to prevent concurrent processing
            var lockAcquired = await TryAcquireLockAsync(lockKey);
            if (!lockAcquired)
            {
                _logger.LogWarning(
                    "Could not acquire lock for message {MessageId}",
                    message.MessageId);
                throw new InvalidOperationException("Message is being processed");
            }

            try
            {
                // Double-check after acquiring lock
                if (await _cache.GetStringAsync(processingKey) != null)
                {
                    return;
                }

                // Process the message
                await _orderService.CreateOrderAsync(
                    message.OrderId,
                    message.CustomerId,
                    message.Amount);

                // Mark as processed (store for 7 days)
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
                };
                await _cache.SetStringAsync(
                    processingKey,
                    DateTime.UtcNow.ToString("O"),
                    cacheOptions);

                _logger.LogInformation(
                    "Successfully processed message {MessageId}",
                    message.MessageId);
            }
            finally
            {
                await ReleaseLockAsync(lockKey);
            }
        }

        private async Task<bool> TryAcquireLockAsync(string lockKey)
        {
            var lockOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            try
            {
                await _cache.SetStringAsync(lockKey, "locked", lockOptions);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task ReleaseLockAsync(string lockKey)
        {
            await _cache.RemoveAsync(lockKey);
        }
    }

}
