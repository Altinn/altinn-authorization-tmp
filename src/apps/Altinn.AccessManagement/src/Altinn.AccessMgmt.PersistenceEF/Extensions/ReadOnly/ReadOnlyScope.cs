namespace Altinn.AccessMgmt.PersistenceEF.Extensions.ReadOnly;

public readonly struct ReadOnlyScope : IDisposable
{
    private readonly IReadOnlyHintService hintService;

    public ReadOnlyScope(IReadOnlyHintService hintService, string? hint = null)
    {
        this.hintService = hintService;
        hintService.SetHint(hint ?? "read_only");
    }

    public void Dispose()
    {
        hintService.ClearHint();
    }
}
