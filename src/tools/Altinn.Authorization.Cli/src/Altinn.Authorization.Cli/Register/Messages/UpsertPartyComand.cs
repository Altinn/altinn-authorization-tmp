using Altinn.Authorization.Cli.ServiceBus.MassTransit;

namespace Altinn.Authorization.Cli.Register.Messages;

sealed record UpsertPartyComand
    : IFakeMassTransitMessage<UpsertPartyComand>
{
    static Utf8String IFakeMassTransitMessage<UpsertPartyComand>.MessageUrn
        => "urn:message:Altinn.Register.PartyImport:UpsertPartyCommand"u8;

    static Guid IFakeMassTransitMessage<UpsertPartyComand>.MessageId(UpsertPartyComand message)
        => message.CommandId;

    public Guid CommandId { get; } = Guid.CreateVersion7();

    public JobTracking Tracking { get; init; }

    public PartialParty Party { get; init; }
}
