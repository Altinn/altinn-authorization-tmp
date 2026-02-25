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
    /// Identifying the resource
    /// </summary>
    public string ResourceId { get; set; }
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
    /// Request status (e.g. draft, pending, approved, rejected, withdrawn)
    /// </summary>
    public RequestStatus Status { get; set; }

    /// <summary>
    /// Relevant links for the request (e.g. confirm, check status)
    /// </summary>
    public RequestLinks Links { get; set; }

    /// <summary>
    /// Connection from one party to another that is requested
    /// </summary>
    public ConnectionRequestDto Connection { get; set; }   
}

public class RequestLinks
{
    /// <summary>
    /// Link for user to confirm request (change status from draft to pending)
    /// </summary>
    public string EnduserLink { get; set; }

    /// <summary>
    /// Link to check status of request
    /// </summary>
    public string StatusLink { get; set; }
}

/// <summary>
/// Definition of connection from one party to another
/// </summary>
public class ConnectionRequestDto
{
    /// <summary>
    /// Party that is requested to grant access
    /// </summary>
    public PartyEntityDto From { get; set; }

    /// <summary>
    /// Party that is access is requested for
    /// </summary>
    public PartyEntityDto To { get; set; }
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
public class PartyEntityDto
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
    /// SubType of entity (e.g. for organization: AS, ENK, DA)
    /// </summary>
    public string SubType { get; set; }

    /// <summary>
    /// OrganizationIdentifier
    /// </summary>
    public string? OrganizationIdentifier { get; set; }

    /// <summary>
    /// PersonIdentifier
    /// </summary>
    public string? PersonIdentifier { get; set; }
}
