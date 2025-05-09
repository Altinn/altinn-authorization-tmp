using Altinn.Authorization.Cli.ServiceBus.MassTransit;

namespace Altinn.Authorization.Cli.Register.Messages;

internal sealed record UpsertValidatedPartyCommand
    : IFakeMassTransitMessage<UpsertValidatedPartyCommand>
{
    static Utf8String IFakeMassTransitMessage<UpsertValidatedPartyCommand>.MessageUrn
        => "urn:message:Altinn.Register.PartyImport:UpsertValidatedPartyCommand"u8;

    static Guid IFakeMassTransitMessage<UpsertValidatedPartyCommand>.MessageId(UpsertValidatedPartyCommand message)
        => message.CommandId;

    public Guid CommandId { get; } = Guid.CreateVersion7();

    public JobTracking Tracking { get; init; }

    public PartialParty Party { get; init; }
}
