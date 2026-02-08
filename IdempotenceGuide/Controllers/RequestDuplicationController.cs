using IdempotenceGuide.Contract;
using IdempotenceGuide.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace IdempotenceGuide.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestDuplicationController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<RequestDuplicationController> _logger;

        // Cache key prefix — centralised so it only lives in one place
        private const string CacheKeyPrefix = "idempotency:payment:";

        // How long a processed result is remembered
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

        public RequestDuplicationController(
            IMemoryCache cache,
            IPaymentService paymentService,
            ILogger<RequestDuplicationController> logger)
        {
            _cache = cache;
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(
            [FromBody] PaymentRequest request,
            [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
        {
            // ── 1. Validate the idempotency key ────────────────────────────
            if (string.IsNullOrEmpty(idempotencyKey))
            {
                _logger.LogWarning("ProcessPayment called without an Idempotency-Key header");
                return BadRequest(new { error = "Idempotency-Key header is required" });
            }

            var cacheKey = $"{CacheKeyPrefix}{idempotencyKey}";

            // ── 2. Return the cached result if this key was already used ───
            if (_cache.TryGetValue(cacheKey, out string cachedJson))
            {
                _logger.LogInformation(
                    "Duplicate request detected for key {IdempotencyKey}. Returning cached result.",
                    idempotencyKey);

                var cachedResponse = JsonSerializer.Deserialize<PaymentResponse>(cachedJson);
                return Ok(cachedResponse);
            }

            // ── 3. Process the payment (first time for this key) ──────────
            try
            {
                var response = await _paymentService.ProcessAsync(request);

                // ── 4. Cache the result with a proper expiration ──────────
                _cache.Set(cacheKey, JsonSerializer.Serialize(response), new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheDuration
                });

                _logger.LogInformation(
                    "Payment processed successfully. TransactionId: {TransactionId}, IdempotencyKey: {IdempotencyKey}",
                    response.TransactionId,
                    idempotencyKey);

                return Ok(response);
            }
            catch (Exception ex)
            {
                // ── 5. Do NOT cache failures ───────────────────────────────
                // The same idempotency key can be retried after an error.
                _logger.LogError(ex,
                    "Payment processing failed for IdempotencyKey: {IdempotencyKey}",
                    idempotencyKey);

                return StatusCode(500, new { error = "Payment processing failed. You may retry with the same Idempotency-Key." });
            }
        }
    }
}