using Altinn.AccessManagement.Core.Extensions;

namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// Assignment
/// </summary>
public class Assignment
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Assignment"/> class.
    /// </summary>
    public Assignment()
    {
        Id = Guid.CreateVersion7();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Assignment"/> class.
    /// </summary>
    /// <param name="id">Id</param>
    public Assignment(Guid id)
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
    /// RoleId
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// FromId
    /// </summary>
    public Guid FromId { get; set; }

    /// <summary>
    /// ToId
    /// </summary>
    public Guid ToId { get; set; }
}

/// <summary>
/// Extended Assignment
/// </summary>
public class ExtAssignment : Assignment
{
    /// <summary>
    /// Role
    /// </summary>
    public Role Role { get; set; }

    /// <summary>
    /// ActiveFrom (Entity)
    /// </summary>
    public Entity From { get; set; }

    /// <summary>
    /// ActiveTo (Entity)
    /// </summary>
    public Entity To { get; set; }
}
