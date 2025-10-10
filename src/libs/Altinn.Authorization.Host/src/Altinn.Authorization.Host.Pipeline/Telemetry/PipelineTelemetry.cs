using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;

namespace Altinn.Authorization.Host.Pipeline.Telemetry;

/// <summary>
/// Telemetry configuration for pipeline operations including distributed tracing and metrics.
/// </summary>
[ExcludeFromCodeCoverage]
internal static partial class PipelineTelemetry
{
    private const string MeterName = "Altinn.Authorization.Host.Pipeline";

    /// <summary>
    /// Used as source for distributed tracing activities.
    /// </summary>
    internal static readonly ActivitySource ActivitySource = new(MeterName);

    /// <summary>
    /// Meter for pipeline metrics.
    /// </summary>
    internal static readonly Meter Meter = new(MeterName);
}
