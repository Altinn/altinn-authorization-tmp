namespace Altinn.AccessMgmt.PersistenceEF.Extensions.Hint;

public readonly struct HintScope : IDisposable
{
    private readonly IHintService hintService;

    public HintScope(IHintService hintService, string hint = null)
    {
        this.hintService = hintService;
        hintService.SetHint(hint ?? "default");
    }

    public void Dispose()
    {
        hintService.ClearHint();
    }
}
