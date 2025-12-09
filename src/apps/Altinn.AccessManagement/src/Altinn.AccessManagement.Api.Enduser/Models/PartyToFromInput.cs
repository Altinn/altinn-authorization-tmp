using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Altinn.AccessManagement.Api.Enduser.Models;

/// <summary>
/// Input for connection controller.
/// </summary>
public class PartyToFromInput : PartyInput
{
    /// <summary>
    /// from party
    /// </summary>
    [FromQuery(Name = "from")]
    [SwaggerSchema(Description = "from", Format = "<me, all | blank, uuid>")]
    public string From { get; set; }

    /// <summary>
    /// to party
    /// </summary>
    [FromQuery(Name = "to")]
    [SwaggerSchema(Description = "to", Format = "<me, all | blank, uuid>")]
    public string To { get; set; }
}
