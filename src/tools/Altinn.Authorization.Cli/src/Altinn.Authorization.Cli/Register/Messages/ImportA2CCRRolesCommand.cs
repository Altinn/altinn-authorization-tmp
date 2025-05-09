using Altinn.Authorization.Cli.ServiceBus.MassTransit;

namespace Altinn.Authorization.Cli.Register.Messages;

sealed record ImportA2CCRRolesCommand
    : IFakeMassTransitMessage<ImportA2CCRRolesCommand>
{
    static Utf8String IFakeMassTransitMessage<ImportA2CCRRolesCommand>.MessageUrn
        => "urn:message:Altinn.Register.PartyImport.A2:ImportA2CCRRolesCommand"u8;

    static Guid IFakeMassTransitMessage<ImportA2CCRRolesCommand>.MessageId(ImportA2CCRRolesCommand message)
        => message.CommandId;

    public Guid CommandId { get; } = Guid.CreateVersion7();

    /// <summary>
    /// Gets the party ID.
    /// </summary>
    /// <remarks>
    /// It's the callers responsibility to ensure that <see cref="PartyId"/> and <see cref="PartyUuid"/>
    /// is for the same party. Failing to do so will result in undefined behavior.
    /// </remarks>
    public required int PartyId { get; init; }

    /// <summary>
    /// Gets the party UUID.
    /// </summary>
    /// <remarks>
    /// It's the callers responsibility to ensure that <see cref="PartyId"/> and <see cref="PartyUuid"/>
    /// is for the same party. Failing to do so will result in undefined behavior.
    /// </remarks>
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
