using static Altinn.AccessMgmt.PersistenceEF.Extensions.Hint.HintService;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions.Hint;

/// <summary>
/// Hint service for connection hinting
/// </summary>
public interface IHintService
{
    /// <summary>
    /// Get the hint
    /// </summary>
    /// <returns></returns>
    HintContext GetHint();

    /// <summary>
    /// Sets a read-only hint for the current async flow and returns a scope that restores the previous hint when disposed.
    /// </summary>
    IDisposable Use(string hint = null);

    /// <summary>
    /// Sets a read-only hint for the current async flow and returns a scope that restores the previous hint when disposed.
    /// </summary>
    IDisposable Use(HintKind kind, string hint = null);

    /// <summary>
    /// Sets a hint without creating a scope. Prefer <see cref="Use"/> in most cases.
    /// </summary>
    void SetHint(string hint);

    /// <summary>
    /// Sets a hint without creating a scope. Prefer <see cref="Use"/> in most cases.
    /// </summary>
    void SetHint(HintKind kind, string hint);

    /// <summary>
    /// Clears any active hint for the current async flow.
    /// </summary>
    void ClearHint();
}
