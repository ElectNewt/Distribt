namespace Distribt.Shared.Communication.Consumer.Manager;

public class ConsumerManager<TMessage> : IConsumerManager<TMessage>
{
    private CancellationTokenSource _cancellationTokenSource;
    
    public ConsumerManager()
    {
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void RestartExecution()
    {
        var cancellationTokenSource = _cancellationTokenSource;
        _cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
    }

    public void StopExecution()
    {
        _cancellationTokenSource.Cancel();
    }

    public CancellationToken GetCancellationToken()
    {
        return _cancellationTokenSource.Token;
    }
}