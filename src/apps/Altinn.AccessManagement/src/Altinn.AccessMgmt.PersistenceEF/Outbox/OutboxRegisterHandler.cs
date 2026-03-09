using System.Collections.Concurrent;
using Altinn.AccessMgmt.PersistenceEF.Models;

namespace Altinn.AccessMgmt.PersistenceEF.Outbox;

/// <summary>
/// 
/// </summary>
internal class OutboxRegisterHandlers
{
    private ConcurrentDictionary<string, Type> Handlers { get; } = [];

    public OutboxRegisterHandlers AddHandler<THandler>(string handlerName)
        where THandler : IOutboxHandler
    {
        Handlers.AddOrUpdate(
            handlerName,
            add => typeof(THandler),
            (_, oldHandler) =>
            {
                return typeof(THandler);
            });

        return this;
    }

    public bool TryGetHandler(string handlerName, out Type handler) =>
        Handlers.TryGetValue(handlerName, out handler);
}

public interface IOutboxHandler
{
    Task Handle<TData>(OutboxMessage message, TData data, IServiceProvider provider, CancellationToken cancellationToken);
}
