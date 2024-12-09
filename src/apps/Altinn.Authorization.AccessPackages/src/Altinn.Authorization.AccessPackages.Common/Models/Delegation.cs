namespace Altinn.Authorization.AccessPackages.Models;

public class Delegation
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
}

public class ExtDelegation : Delegation
{
    public Assignment Assignment { get; set; }
}
