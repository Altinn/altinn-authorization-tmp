using Altinn.Authorization.Cli.ServiceBus.MassTransit;

namespace Altinn.Authorization.Cli.Register.Messages;

internal sealed record ResolveAndUpsertA2CCRRoleAssignmentsCommand
    : IFakeMassTransitMessage<ResolveAndUpsertA2CCRRoleAssignmentsCommand>
{
    static Utf8String IFakeMassTransitMessage<ResolveAndUpsertA2CCRRoleAssignmentsCommand>.MessageUrn
        => "urn:message:Altinn.Register.PartyImport.A2:ResolveAndUpsertA2CCRRoleAssignmentsCommand"u8;

    static Guid IFakeMassTransitMessage<ResolveAndUpsertA2CCRRoleAssignmentsCommand>.MessageId(ResolveAndUpsertA2CCRRoleAssignmentsCommand message)
        => message.CommandId;

    public Guid CommandId { get; } = Guid.CreateVersion7();

    public JobTracking Tracking { get; init; }

    public Guid FromPartyUuid { get; init; }

    public int FromPartyId { get; init; }
}
