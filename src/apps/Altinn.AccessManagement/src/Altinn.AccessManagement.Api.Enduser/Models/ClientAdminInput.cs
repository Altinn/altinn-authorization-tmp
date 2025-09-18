using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Altinn.AccessManagement.Api.Enduser.Models;

/// <summary>
/// Input for connection controller.
/// </summary>
public class ClientAdminInput
{
    /// <summary>
    /// making request on behalf of.
    /// </summary>
    [FromQuery(Name = "party")]
    [SwaggerSchema(Description = "party", Format = "<uuid>")]
    public string Party { get; set; }

    /// <summary>
    /// from party
    /// </summary>
    [FromQuery(Name = "client")]
    [SwaggerSchema(Description = "client", Format = "<all | blank, uuid>")]
    public string Client { get; set; }

    /// <summary>
    /// to party
    /// </summary>
    [FromQuery(Name = "agent")]
    [SwaggerSchema(Description = "agent", Format = "<all | blank, uuid>")]
    public string Agent { get; set; }
}
