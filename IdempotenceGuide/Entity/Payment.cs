namespace IdempotenceGuide.Entity
{
    public class Payment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string CustomerId { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
