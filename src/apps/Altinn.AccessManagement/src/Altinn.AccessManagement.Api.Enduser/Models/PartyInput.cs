using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Altinn.AccessManagement.Api.Enduser.Models;

/// <summary>
/// Input for connection controller.
/// </summary>
public class PartyInput
{
    /// <summary>
    /// making request on behalf of.
    /// </summary>
    [Required]
    [FromQuery(Name = "party")]
    [SwaggerSchema(Description = "party", Format = "<me, uuid>")]
    public string Party { get; set; }
}
