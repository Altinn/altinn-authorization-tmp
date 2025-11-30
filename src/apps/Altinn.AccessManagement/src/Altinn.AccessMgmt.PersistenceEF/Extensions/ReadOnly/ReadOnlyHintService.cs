namespace Altinn.AccessMgmt.PersistenceEF.Extensions.ReadOnly;

public class ReadOnlyHintService : IReadOnlyHintService
{
    private string _hint;

    public void SetHint(string name)
    {
        _hint = name;
    }

    public string GetHint()
    {
        return _hint;
    }
}
