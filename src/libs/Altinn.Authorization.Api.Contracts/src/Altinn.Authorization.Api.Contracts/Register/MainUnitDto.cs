using System.ComponentModel.DataAnnotations;

namespace Altinn.Authorization.Api.Contracts.Register;

/// <summary>
/// Sub-unit information DTO
/// </summary>
public class SubUnitDto
{
    [Required]
    public string OrganizationNumber { get; set; } = string.Empty;
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? BusinessAddress { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime RegistrationDate { get; set; }
}

/// <summary>
/// Additional company details DTO
/// </summary>
public class CompanyDetailsDto
{
    public string? CompanyForm { get; set; }
    public string? ShareCapital { get; set; }
    public string? Currency { get; set; }
    public DateTime? LastAccountsDate { get; set; }
    public List<string>? BusinessCodes { get; set; }
    public Dictionary<string, string>? RegisterDetails { get; set; }
}

/// <summary>
/// Main unit information DTO
/// </summary>
public class MainUnitDto
{
    [Required]
    public string OrganizationNumber { get; set; } = string.Empty;
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? BusinessAddress { get; set; }
    public string? PostalAddress { get; set; }
    public string? IndustryCode { get; set; }
    public string? IndustryDescription { get; set; }
    public int? EmployeeCount { get; set; }
    public DateTime? FoundedDate { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    public List<SubUnitDto> SubUnits { get; set; } = [];
    public CompanyDetailsDto? CompanyDetails { get; set; }
}

/// <summary>
/// Main unit lookup request DTO
/// </summary>
public class MainUnitLookupDto
{
    public string? OrganizationNumber { get; set; }
    public string? Name { get; set; }
    public string? IndustryCode { get; set; }
    public bool IncludeSubUnits { get; set; } = false;
    public bool IncludeCompanyDetails { get; set; } = false;
    public bool IncludeInactive { get; set; } = false;
}