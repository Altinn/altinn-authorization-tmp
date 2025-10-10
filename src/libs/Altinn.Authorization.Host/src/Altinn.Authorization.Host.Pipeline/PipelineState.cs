using Altinn.Authorization.Host.Lease;

namespace Altinn.Authorization.Host.Pipeline;

/// <summary>
/// Shared state for a pipeline execution.
/// </summary>
public class PipelineState
{
    /// <summary>
    /// The distributed lease associated with this pipeline, if configured.
    /// </summary>
    public ILease? Lease { get; internal set; }
}
