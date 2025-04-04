using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorization.Host.Operations;

public class ServiceObjectResultConverter<TContent, TConvert>(ServiceObjectResult<TContent> operationResult)
{
    private ServiceObjectResult<TContent> OperationResult { get; } = operationResult;

    private Func<ServiceObjectResult<TContent>, TConvert>? _handler;

    public ServiceObjectResultConverter<TContent, TConvert> On<TOperationResult>(Func<TOperationResult, TConvert> handler)
        where TOperationResult : ServiceObjectResult<TContent>
    {
        if (OperationResult is TOperationResult typedResult)
        {
            // Assigning the handler safely using Interlocked
            Interlocked.Exchange(ref _handler, _ => handler(typedResult));
        }

        return this;
    }

    public TConvert Convert()
    {
        if (_handler is not null)
        {
            return _handler(OperationResult);
        }

        throw new InvalidOperationException($"No converter registered for object result of type '{OperationResult.GetType().Name}'");
    }
}
