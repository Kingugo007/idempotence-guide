using IdempotenceGuide.Entity;

namespace IdempotenceGuide.Contract
{
    public interface IMessageProcessor
    {
        Task ProcessMessageAsync(OrderMessage message);
    }
}
