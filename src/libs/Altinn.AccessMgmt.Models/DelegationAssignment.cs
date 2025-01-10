namespace Altinn.AccessMgmt.Models;

public class DelegationAssignment
{
    public Guid Id { get; set; }
    public Guid DelegationId { get; set; }
    public Guid AssignmentId { get; set; }
}
public class ExtDelegationAssignment : DelegationAssignment
{
    public Delegation Delegation { get; set; }
    public Assignment Assignment { get; set; }
}
