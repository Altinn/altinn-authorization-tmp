namespace Altinn.AccessMgmt.Models;

/// <summary>
/// Delegation between two assignments
/// </summary>
public class Delegation
{
    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Assignment to delegate from
    /// </summary>
    public Guid FromId { get; set; }

    /// <summary>
    /// Assignment to delegate to
    /// </summary>
    public Guid ToId { get; set; }

    /// <summary>
    /// Entity between from and to
    /// </summary>
    public Guid ViaId { get; set; }

    /// <summary>
    /// Entity origin
    /// </summary>
    public Guid SourceId { get; set; }
}

/// <summary>
/// Extended delegation
/// </summary>
public class ExtDelegation : Delegation
{
    /// <summary>
    /// Assignment to delegate from
    /// </summary>
    public Assignment From { get; set; }

    /// <summary>
    /// Assignment to delegate to
    /// </summary>
    public Assignment To { get; set; }

    /// <summary>
    /// Entity between from and to
    /// </summary>
    public Entity Via { get; set; }

    /// <summary>
    /// Entity origin
    /// </summary>
    public Entity Source { get; set; }
}
