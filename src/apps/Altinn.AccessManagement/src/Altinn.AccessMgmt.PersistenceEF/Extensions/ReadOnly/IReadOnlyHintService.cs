namespace Altinn.AccessMgmt.PersistenceEF.Extensions.ReadOnly;

public interface IReadOnlyHintService
{
    void SetHint(string name);

    string GetHint();
}
