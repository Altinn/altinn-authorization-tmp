using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorization.Api.Contracts.AccessManagement;

/// <summary>
/// Properties for querying requests
/// </summary>
public class RequestQueryInput
{
    /// <summary>
    /// Urn describing the party
    /// </summary>
    [FromQuery(Name = "from")]
    public string From { get; set; }

    /// <summary>
    /// Urn describing the party
    /// </summary>
    [FromQuery(Name = "to")]
    public string To { get; set; }

    /// <summary>
    /// Urn describing the party
    /// </summary>
    [FromQuery(Name = "party")]
    public string Party { get; set; }
}

/// <summary>
/// Base input dto for creating a new request
/// </summary>
public class RequestInput
{
    /// <summary>
    /// Urn describing the party
    /// </summary>
    public string From { get; set; }

    /// <summary>
    /// Urn describing the party
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// Urn describing the party
    /// </summary>
    public string Party { get; set; }
}

/// <summary>
/// Extension of RequestInput for creating a new request for access package
/// </summary>
public class RequestPackageInput : RequestInput
{
    /// <summary>
    /// Urn describing the package
    /// </summary>
    public PackageRefrenceDto Package { get; set; }
}

/// <summary>
/// Extension of RequestInput for creating a new request for resource
/// </summary>
public class RequestResourceInput : RequestInput
{
    /// <summary>
    /// Urn describing the package
    /// </summary>
    public ResourceRefrenceDto Resource { get; set; }
}

/// <summary>
/// Resource reference
/// </summary>
public class ResourceRefrenceDto
{
    /// <summary>
    /// Urn identifying the resource
    /// </summary>
    public string Resource { get; set; }
}

/// <summary>
/// Access package reference
/// </summary>
public class PackageRefrenceDto
{
    /// <summary>
    /// Urn identifying the package
    /// </summary>
    public string Package { get; set; }
}

/// <summary>
/// Base response dto for requests
/// </summary>
public class RequestDto
{
    /// <summary>
    /// Request id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Request status (e.g. pending, approved, rejected, withdrawn)
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Party that is requested to grant access
    /// </summary>
    public EntityDto From { get; set; }

    /// <summary>
    /// Party that is access is requested for
    /// </summary>
    public EntityDto To { get; set; }
}

/// <summary>
/// Extension of RequestDto for resource request
/// </summary>
public class RequestResourceDto : RequestDto
{
    /// <summary>
    /// Resource that is requested
    /// </summary>
    public ResourceRefrenceDto Resource { get; set; }
}

/// <summary>
/// Extension of RequestDto for access package request
/// </summary>
public class RequestPackageDto : RequestDto
{
    /// <summary>
    /// Access package that is requested
    /// </summary>
    public PackageRefrenceDto Package { get; set; }
}

/// <summary>
/// Party entity refrence
/// </summary>
public class EntityDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Type of entity (e.g. organization, person, systemuser)
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Variant of entity (e.g. for organization: AS, ENK, DA)
    /// </summary>
    public string Variant { get; set; }
}
