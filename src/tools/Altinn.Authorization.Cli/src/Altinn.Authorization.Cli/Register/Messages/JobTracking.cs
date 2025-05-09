namespace Altinn.Authorization.Cli.Register.Messages;

internal sealed record JobTracking
{
    public string JobName { get; init; }

    public uint Progress { get; init; }
}
