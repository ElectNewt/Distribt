namespace Distribt.Shared.Communication.Consumer;

public interface IMessageConsumer
{
    Task StartAsync(CancellationToken cancelToken = default);
}

public interface IMessageConsumer<T> : IMessageConsumer
{
}