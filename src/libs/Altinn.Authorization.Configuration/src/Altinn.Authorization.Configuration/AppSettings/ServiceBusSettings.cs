using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.Configuration.AppSettings;

/// <summary>
/// Configuration settings for Azure Service Bus.
/// This class holds the settings required to connect to the Azure Service Bus service.
/// </summary>
[ExcludeFromCodeCoverage]
public class ServiceBusSettings
{
    /// <summary>
    /// The URI of the Azure Service Bus endpoint.
    /// This endpoint is used to send and receive messages from the Azure Service Bus service.
    /// </summary>
    public Uri Endpoint { get; set; }
}
