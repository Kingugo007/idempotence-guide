using IdempotenceGuide.Entity;

namespace IdempotenceGuide.Contract
{
    public interface IOrderService
    {
        Task CreateOrderAsync(string orderId, string customerId, decimal amount);
    }
}
