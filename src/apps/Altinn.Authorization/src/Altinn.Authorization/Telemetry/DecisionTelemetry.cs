using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;

namespace Altinn.Platform.Authorization.Telemetry
{
    /// <summary>
    /// Telemetry definitions for the Policy Decision Point, exposing metrics that attribute PDP
    /// usage back to the resource owner so that volume and cost can be allocated per service owner.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class DecisionTelemetry
    {
        /// <summary>
        /// Meter name registered with the OpenTelemetry meter provider in Program.cs. Any change
        /// here must be reflected in the <c>AddMeter</c> call, otherwise the instrument will be
        /// silently dropped.
        /// </summary>
        internal const string MeterName = "Altinn.Authorization.Pdp";

        /// <summary>
        /// Dimension value used when an owner or resource identifier cannot be resolved.
        /// </summary>
        internal const string UnknownDimensionValue = "unknown";

        private const string OwnerOrgTag = "owner_org";
        private const string ResourceIdTag = "resource_id";

        private static readonly Meter Meter = new(MeterName);

        /// <summary>
        /// Counts PDP authorization decisions, dimensioned by the resource owner organization and
        /// the resource identifier.
        /// </summary>
        internal static readonly Counter<long> PdpDecisions =
            Meter.CreateCounter<long>("pdp.decisions", unit: "1", description: "Number of PDP authorization decisions evaluated");

        /// <summary>
        /// Records a single PDP decision with the given owner and resource identifier.
        /// </summary>
        internal static void RecordDecision(string ownerOrg, string resourceId)
        {
            PdpDecisions.Add(
                1,
                new KeyValuePair<string, object?>(OwnerOrgTag, ownerOrg),
                new KeyValuePair<string, object?>(ResourceIdTag, resourceId));
        }
    }
}
