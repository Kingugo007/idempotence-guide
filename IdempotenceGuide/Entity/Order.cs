namespace IdempotenceGuide.Entity
{
    public class OrderMessage
    {
        public string MessageId { get; set; }
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
    }

    public class Order
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }

}
