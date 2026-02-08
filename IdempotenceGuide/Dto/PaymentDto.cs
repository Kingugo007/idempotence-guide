namespace IdempotenceGuide.Dto
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string CustomerId { get; set; }
    }

    public class PaymentResponse
    {
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
