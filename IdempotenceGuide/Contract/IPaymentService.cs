using IdempotenceGuide.Dto;

namespace IdempotenceGuide.Contract
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessAsync(PaymentRequest paymentRequest);
    }
}
