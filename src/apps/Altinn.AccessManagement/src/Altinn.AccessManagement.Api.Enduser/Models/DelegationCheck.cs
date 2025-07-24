using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Altinn.AccessManagement.Api.Enduser.Models;

/// <summary>
/// Input for connection controller.
/// </summary>
public class DelegationCheck
{
    public Guid Id { get; set; }
    
    public bool CanAssign { get; set; }
}
