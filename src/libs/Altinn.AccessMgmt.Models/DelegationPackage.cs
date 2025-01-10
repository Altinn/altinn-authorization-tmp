namespace Altinn.AccessMgmt.Models;

public class DelegationPackage
{
    public Guid Id { get; set; }
    public Guid DelegationId { get; set; }
    public Guid PackageId { get; set; }
}
public class ExtDelegationPackage : DelegationPackage
{
    public Delegation Delegation { get; set; }
    public Package Package { get; set; }
}
