using IdempotenceGuide.Contract;
using IdempotenceGuide.Entity;
using IdempotenceGuide.Persistence;

namespace IdempotenceGuide.Service
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateOrderAsync(string orderId, string customerId, decimal amount)
        {
            var order = new Order()
            {
                Amount = amount,
                CustomerId = customerId,
                Id = orderId
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

        }
    }
}
