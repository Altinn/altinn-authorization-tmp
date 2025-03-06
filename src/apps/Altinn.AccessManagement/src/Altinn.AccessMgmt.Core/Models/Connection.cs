using System.Security.Principal;

namespace Altinn.AccessMgmt.Core.Models;

public class Connection 
{
    /// <summary>
    /// Identity, either assignment og delegation
    /// </summary>
    public Guid Id { get; set; } // AssignmentId or DelegationId

    /// <summary>
    /// The entity identity the connection is from (origin, client, etc) 
    /// </summary>
    public Guid FromId { get; set; }

    /// <summary>
    /// The role used for the assignment between from and to -or- between from and facilitator (when delegated)
    /// </summary>
    public Guid FromRoleId { get; set; }

    /// <summary>
    /// The entity betweeen from and to. When connection is delegated.
    /// </summary>
    public Guid? FacilitatorId { get; set; }

    /// <summary>
    /// The entity identity the connection is to (destination, agent, etc)
    /// </summary>
    public Guid ToId { get; set; }

    /// <summary>
    /// The role identity used for the assignment between facilitator and to. When connection is delegated.
    /// </summary>
    public Guid? ToRoleId { get; set; }
}

public class ExtConnection : Connection
{
    /// <summary>
    /// 
    /// </summary>
    public Entity From { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Role FromRole { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Entity Facilitator { get; set; } 

    /// <summary>
    /// 
    /// </summary>
    public Entity To { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Role ToRole { get; set; }
}
