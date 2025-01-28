using System.Diagnostics.CodeAnalysis;
using MassTransit;

namespace Altinn.Authorization.Host.MassTransit;

/// <summary>
/// Provides the client configuration settings for connecting to a MassTransit bus.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class MassTransitOptions
{
    /// <summary>
    /// Gets or sets a boolean value that indicates whether the bus health check is disabled or not.
    /// </summary>
    /// <value>
    /// The default value is <see langword="false"/>.
    /// </value>
    public bool DisableHealthChecks { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the OpenTelemetry tracing is disabled or not.
    /// </summary>
    /// <value>
    /// The default value is <see langword="false"/>.
    /// </value>
    public bool DisableTracing { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the OpenTelemetry metrics are disabled or not.
    /// </summary>
    /// <value>
    /// The default value is <see langword="false"/>.
    /// </value>
    public bool DisableMetrics { get; set; }

    /// <summary>
    /// Gets or sets the transport to use for the bus.
    /// </summary>
    public MassTransitTransport Transport { get; set; }

    /// <summary>
    /// Gets or sets the Azure Service Bus specific settings.
    /// </summary>
    public MassTransitAzureServiceBusSettings AzureServiceBus { get; set; } = new();

    public MassTransitInMemorySettings InMemorySettings { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    public class MassTransitInMemorySettings
    {
        /// <summary>
        /// /
        /// </summary>
        public Action<IBusRegistrationContext, IInMemoryBusFactoryConfigurator> Configure { get; set; }
    }

    /// <summary>
    /// Provides the client configuration settings for connecting to an Azure Service Bus.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class MassTransitAzureServiceBusSettings
    {
        public string Endpoint { get; set; }

        public Action<IBusRegistrationContext, IServiceBusBusFactoryConfigurator> Configure { get; set; }
    }

}

/// <summary>
/// Mass transit transports.
/// </summary>
public enum MassTransitTransport
{
    /// <summary>
    /// In memory transport. Only used for testing.
    /// </summary>
    InMemory = default,

    /// <summary>
    /// Azure Service Bus transport.
    /// </summary>
    AzureServiceBus,
}
