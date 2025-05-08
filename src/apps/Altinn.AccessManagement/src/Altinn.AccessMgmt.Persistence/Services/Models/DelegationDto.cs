using Altinn.AccessMgmt.Core.Models;

namespace Altinn.AccessMgmt.Persistence.Services.Models;

/// <summary>
/// Delegation between assignments
/// </summary>
public class DelegationDto
{
    /// <summary>
    /// Identity, either assignment og delegation
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Assignment to delegate from
    /// </summary>
    public Assignment From { get; set; }

    /// <summary>
    /// Assignment to delegate to
    /// </summary>
    public Assignment To { get; set; }

    /// <summary>
    /// Delegation facilitator
    /// </summary>
    public Entity Facilitator { get; set; }
}
