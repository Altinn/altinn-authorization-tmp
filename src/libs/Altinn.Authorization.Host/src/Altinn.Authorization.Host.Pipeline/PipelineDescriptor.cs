using Altinn.Authorization.Host.Pipeline.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.Host.Pipeline;

internal class PipelineDescriptor(PipelineGroup descriptor) : IPipelineDescriptor
{
    public string Name { get; private set; }

    internal PipelineSourceBuilder Source { get; private set; }

    internal Func<IServiceProvider, IServiceScope> ServiceScope { get; set; }

    internal string LeaseName { get; private set; }

    public IPipelineDescriptor WithName(string name)
    {
        Name = name;
        return this;
    }

    public IPipelineDescriptor WithLease(string lease)
    {
        LeaseName = lease;
        return this;
    }

    public IPipelineDescriptor WithServiceScope(Func<IServiceProvider, IServiceScope> func)
    {
        ServiceScope = func;
        return this;
    }

    public ISourceBuilder WithStages()
    {
        Source = new PipelineSourceBuilder(descriptor);
        return Source;
    }
}

public interface IPipelineDescriptor
{
    public string Name { get; }

    IPipelineDescriptor WithLease(string lease);

    IPipelineDescriptor WithServiceScope(Func<IServiceProvider, IServiceScope> func);

    ISourceBuilder WithStages();
}