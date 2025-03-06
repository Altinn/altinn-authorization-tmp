namespace Altinn.AccessMgmt.Core.Models;

public class Connection 
{
    public Guid Id { get; set; } // AssignmentId or DelegationId
    
    public Guid FromId { get; set; }
    public Guid RoleId { get; set; }
    public Guid ToId { get; set; }

    
    public Guid? FacilitatorId { get; set; } // Only for Delegations
    public Guid? FacilitatorRoleId { get; set; } // Only for Delegations
}

public class ExtConnection : Connection
{
    public Entity From { get; set; }
    public Role FromRole { get; set; }
    public Entity Facilitator { get; set; } 
    public Entity To { get; set; }
    public Role ToRole { get; set; }
}
