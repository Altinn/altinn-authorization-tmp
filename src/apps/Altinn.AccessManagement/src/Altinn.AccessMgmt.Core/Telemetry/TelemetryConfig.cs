using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Altinn.AccessMgmt.Core;

public static class TelemetryConfig
{
    public const string ServiceName = "Altinn.AccessMgmt";

    public const string ServiceVersion = "1.0.0";

    public static ActivitySource ActivitySource { get; } = new(ServiceName);

    public static Meter Meter { get; } = new(ServiceName, ServiceVersion);
}
