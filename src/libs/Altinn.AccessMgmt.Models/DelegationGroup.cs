namespace Altinn.AccessMgmt.Models;

public class DelegationGroup
{
    public Guid Id { get; set; }
    public Guid DelegationId { get; set; }
    public Guid GroupId { get; set; }
}
public class ExtDelegationGroup : DelegationGroup
{
    public Delegation Delegation { get; set; }
    public EntityGroup Group { get; set; }
}
