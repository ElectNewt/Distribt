using Distribt.Services.Subscriptions.Dtos;


namespace Distribt.Services.Subscriptions.Consumer.Handler;

public class SubscriptionHandler : IIntegrationMessageHandler<SubscriptionDto>
{
    private readonly IDependenciaTest _dependencia;

    public SubscriptionHandler(IDependenciaTest dependencia)
    {
        _dependencia = dependencia;
    }
    public Task Handle(IntegrationMessage<SubscriptionDto> message, CancellationToken cancelToken = default(CancellationToken))
    {
        int result = _dependencia.Execute();
       Console.WriteLine($"Email {message.Content.Email} successfully subscribed. y la dependencia es {result}");
       return Task.CompletedTask;
    }
}

public interface IDependenciaTest
{
    int Execute();
}

public class DependenciaTest : IDependenciaTest
{
    public int Execute()
    {
        return 1;
    }   
}