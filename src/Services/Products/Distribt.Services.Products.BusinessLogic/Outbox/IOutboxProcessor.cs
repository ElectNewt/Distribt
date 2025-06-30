namespace Distribt.Services.Products.BusinessLogic.Outbox;

public interface IOutboxProcessor
{
    Task ProcessPendingMessages(CancellationToken cancellationToken = default);
}