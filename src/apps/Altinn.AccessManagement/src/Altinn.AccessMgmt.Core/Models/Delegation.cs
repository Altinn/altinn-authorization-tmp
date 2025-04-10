using Altinn.AccessManagement.Core.Extensions;

namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// Delegation between two assignments
/// </summary>
public class Delegation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Delegation"/> class.
    /// </summary>
    public Delegation()
    {
        Id = Guid.CreateVersion7();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Delegation"/> class.
    /// </summary>
    /// <param name="id">Id</param>
    public Delegation(Guid id)
    {
        if (!id.IsVersion7Uuid())
        {
            throw new ArgumentException("Id must be a version 7 UUID", nameof(id));
        }

        Id = id;
    }

    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Assignment to delegate from
    /// </summary>
    public Guid FromId { get; set; }

    /// <summary>
    /// Assignment to delegate to
    /// </summary>
    public Guid ToId { get; set; }

    /// <summary>
    /// Entity owner of the Delegation
    /// </summary>
    public Guid FacilitatorId { get; set; }
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
    /// Delegation facilitator
    /// </summary>
    public Entity Facilitator { get; set; }
}
