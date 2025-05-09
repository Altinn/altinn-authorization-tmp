namespace Altinn.Authorization.Cli.Register.Messages;

internal sealed record PartialParty
{
    public Guid PartyUuid { get; init; }
}
