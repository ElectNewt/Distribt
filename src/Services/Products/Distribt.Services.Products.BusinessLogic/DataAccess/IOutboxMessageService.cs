namespace Distribt.Services.Products.BusinessLogic.DataAccess;

public interface IOutboxMessageService
{
    Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default);
} 