using IdempotenceGuide.Contract;
using IdempotenceGuide.Dto;
using IdempotenceGuide.Entity;
using IdempotenceGuide.Persistence;

namespace IdempotenceGuide.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentResponse> ProcessAsync(PaymentRequest paymentRequest)
        {
            var payment = new Payment
            {
                Amount = paymentRequest.Amount,
                Currency = paymentRequest.Currency,
                CustomerId = paymentRequest.CustomerId,
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            var response = new PaymentResponse { ProcessedAt = payment.DateCreated, Status = "Success", TransactionId = payment.Id };

            return response;

        }
    }
}
