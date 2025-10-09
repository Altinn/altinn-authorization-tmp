using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.Host.Lease.Telemetry;

/// <summary>
/// Config to be used for Telemetry in Altinn.AccessManagement.Persistence
/// </summary>
[ExcludeFromCodeCoverage]
internal static partial class PipelineTelemetry
{
    /// <summary>
    /// Used as source for the current project.
    /// </summary>
    internal static readonly ActivitySource ActivitySource = new("Altinn.Authorization.Host.Pipeline");
}
