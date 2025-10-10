using System.Diagnostics;
using Altinn.Authorization.Host.Lease;

namespace Altinn.Authorization.Host.Pipeline;

public record PipelineSingleMessage<T>(
    T Data,
    ActivityContext? ActivityContext,
    CancellationTokenSource CancellationTokenSource)
{
}

public class PipelineState
{
    public ILease? Lease { get; internal set; }
}
