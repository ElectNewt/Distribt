using System.Collections.Concurrent;

namespace Distribt.Shared.Communication.Consumer.Handler;

public interface IMessageHandlerRegistry
{
    IEnumerable<IMessageHandler> GetMessageHandlerForType(Type messageHandlerType, Type messageType);
}

public class MessageHandlerRegistry : IMessageHandlerRegistry
{
    private readonly IEnumerable<IMessageHandler> _messageHandlers;

    private readonly ConcurrentDictionary<string, IEnumerable<IMessageHandler>> _cachedHandlers =
        new ConcurrentDictionary<string, IEnumerable<IMessageHandler>>();

    public MessageHandlerRegistry(IEnumerable<IMessageHandler> messageHandlers)
    {
        _messageHandlers = messageHandlers;
    }

    public IEnumerable<IMessageHandler> GetMessageHandlerForType(Type messageHandlerType, Type messageType)
    {
        var key = $"{messageHandlerType}-{messageType}";
        if (_cachedHandlers.TryGetValue(key, out var existingHandlers))
        {
            return existingHandlers;
        }

        IList<IMessageHandler> handlers =
            GetMessageHandlersInternal(messageHandlerType, messageType);
            
        _cachedHandlers.AddOrUpdate(key, handlers.Distinct(), (_, __) => handlers);
        if (handlers.Count == 0)
        {
            // #4 add logging and specify no handlers found.
        }

        return handlers;
    }

    private IList<IMessageHandler> GetMessageHandlersInternal(Type messageHandlerType, Type messageType)
    {
        return
            _messageHandlers.Where(
                    h => h.GetType()
                        .GetInterfaces()
                        .Contains(messageHandlerType))
                .Distinct()
                .ToList();
    }
}