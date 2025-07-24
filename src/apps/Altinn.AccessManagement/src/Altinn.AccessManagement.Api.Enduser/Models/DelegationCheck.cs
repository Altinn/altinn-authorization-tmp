namespace Altinn.AccessManagement.Api.Enduser.Models;

/// <summary>
/// Delegation Check Api Model
/// </summary>
public class DelegationCheck
{
    public Guid Id { get; set; }
    
    public bool CanAssign { get; set; }
}
