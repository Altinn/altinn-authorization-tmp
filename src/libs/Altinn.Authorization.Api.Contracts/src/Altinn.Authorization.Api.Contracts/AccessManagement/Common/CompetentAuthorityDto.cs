namespace Altinn.Authorization.Api.Contracts.AccessManagement.Common;

/// <summary>
/// Competent authority DTO
/// </summary>
public class CompetentAuthorityDto
{
    public string? Orgcode { get; set; }
    public string? Organization { get; set; }
    public string? Name { get; set; }
}