namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Filter inputs for authorized parties controller.
/// </summary>
public class AuthorizedPartiesFilters
{
    public bool IncludeAltinn2 { get; set; } = true;

    public bool IncludeAltinn3 { get; set; } = true;

    public string ProviderCode { get; set; } = null;

    public string[] AnyOfResourceIds { get; set; } = null;

    public string[] AllOfResourceIds { get; set; } = null;
}
