using Altinn.Authorization.Cli.ServiceBus.MassTransit;

namespace Altinn.Authorization.Cli.Register.Messages;

sealed record ImportA2PartyCommand
    : IFakeMassTransitMessage<ImportA2PartyCommand>
{
    static Utf8String IFakeMassTransitMessage<ImportA2PartyCommand>.MessageUrn
        => "urn:message:Altinn.Register.PartyImport.A2:ImportA2PartyCommand"u8;

    static Guid IFakeMassTransitMessage<ImportA2PartyCommand>.MessageId(ImportA2PartyCommand message)
        => message.CommandId;

    public Guid CommandId { get; } = Guid.CreateVersion7();

    /// <summary>
    /// Gets the party UUID.
    /// </summary>
    public required Guid PartyUuid { get; init; }

    /// <summary>
    /// Gets the change ID.
    /// </summary>
    public required uint ChangeId { get; init; }

    /// <summary>
    /// Gets when the change was registered.
    /// </summary>
    public required DateTimeOffset ChangedTime { get; init; }
}
