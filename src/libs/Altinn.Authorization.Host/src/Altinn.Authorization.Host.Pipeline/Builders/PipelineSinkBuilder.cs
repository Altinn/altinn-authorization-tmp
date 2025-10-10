namespace Altinn.Authorization.Host.Pipeline.Builders;

internal class PipelineSinkBuilder<TIn>(PipelineGroup descriptor) : ISinkBuilder<TIn>
{
    /// <inheritdoc/>
    public IPipelineGroup Build()
    {
        return descriptor;
    }
}

/// <summary>
/// Builder for completing pipeline configuration.
/// </summary>
/// <typeparam name="TIn">The sink input message type.</typeparam>
public interface ISinkBuilder<TIn>
{
    /// <summary>
    /// Completes the pipeline configuration and returns to the parent group.
    /// </summary>
    /// <returns>The parent group for additional configuration.</returns>
    IPipelineGroup Build();
}
