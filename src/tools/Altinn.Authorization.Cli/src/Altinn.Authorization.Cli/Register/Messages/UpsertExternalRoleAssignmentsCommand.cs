using Altinn.Authorization.Cli.ServiceBus.MassTransit;

namespace Altinn.Authorization.Cli.Register.Messages;

internal sealed record UpsertExternalRoleAssignmentsCommand
    : IFakeMassTransitMessage<UpsertExternalRoleAssignmentsCommand>
{
    static Utf8String IFakeMassTransitMessage<UpsertExternalRoleAssignmentsCommand>.MessageUrn
        => "urn:message:Altinn.Register.PartyImport:UpsertExternalRoleAssignmentsCommand"u8;

    static Guid IFakeMassTransitMessage<UpsertExternalRoleAssignmentsCommand>.MessageId(UpsertExternalRoleAssignmentsCommand message)
        => message.CommandId;

    public Guid CommandId { get; } = Guid.CreateVersion7();

    /// <summary>
    /// Gets the party from which to upsert the external roles from.
    /// </summary>
    public Guid FromPartyUuid { get; init; }

    /// <summary>
    /// Gets the party ID that the role assignments are from.
    /// </summary>
    /// <remarks>
    /// This was added later to make dealing with errors easier, as such, older messages
    /// does not contain this value and will default to 0. This is why this property is
    /// not marked as required as of now.
    /// </remarks>
    public int FromPartyId { get; init; }

    /// <summary>
    /// Gets the tracking information for the upsert.
    /// </summary>
    public JobTracking Tracking { get; init; }
}
