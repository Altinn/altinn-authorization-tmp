using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Connection;

/// <summary>
/// Input for connection controller.
/// </summary>
public class ConnectionInputDto
{
    /// <summary>
    /// making request on behalf of.
    /// </summary>
    [FromQuery(Name = "party")]
    [SwaggerSchema(Description = "party", Format = "<me, uuid>")]
    public string Party { get; set; }

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
