using Altinn.Platform.Register.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Altinn.AccessManagement.Api.Enduser.Models;

/// <summary>
/// Input for connection controller.
/// </summary>
public class PartyToInput : PartyInput 
{
    /// <summary>
    /// making request on behalf of.
    /// </summary>
    [FromQuery(Name = "to")]
    [SwaggerSchema(Description = "party", Format = "<me, uuid>")]
    public string To { get; set; }
}
