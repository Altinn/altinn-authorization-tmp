using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Altinn.AccessManagement.Api.Enduser.Models;

/// <summary>
/// Page Inputs.
/// </summary>
public class PagingInput : IExamplesProvider<PagingInput>
{
    /// <summary>
    /// Number of elements requested from page. 
    /// </summary>
    [FromHeader(Name = "X-Page-Size")]
    [DefaultValue(100)]
    [SwaggerSchema(Description = "Page Size", Format = "[0, 100]")]
    public uint PageSize { get; set; } = 100;

    /// <summary>
    /// Number of paged skipped
    /// </summary>
    [DefaultValue(0)]
    [FromHeader(Name = "X-Page-Number")]
    [SwaggerSchema(Description = "Page Number", Format = "[0, ∞)")]
    public uint PageNumber { get; set; } = 0;

    /// <summary>
    /// Which Lanauage
    /// </summary>
    [DefaultValue("nb")]
    [FromHeader(Name = "X-Language")]
    [SwaggerSchema(Description = "Language", Format = "(nb,nn,en)")]
    public string Language { get; set; }

    /// <inheritdoc/>
    public PagingInput GetExamples()
    {
        return new PagingInput
        {
            PageNumber = 2,
            PageSize = 56,
            Language = "en",
        };
    }
}
