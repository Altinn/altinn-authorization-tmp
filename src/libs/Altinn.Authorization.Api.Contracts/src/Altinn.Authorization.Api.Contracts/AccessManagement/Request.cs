using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorization.Api.Contracts.AccessManagement;

/// <summary>
/// Properties for querying requests (service owner)
/// </summary>
public class RequestServiceOwnerQuery
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
/// Properties for querying requests (end user)
/// </summary>
public class RequestEnduserQuery
{
    /// <summary>
    /// Party acting on behalf of (uuid)
    /// </summary>
    [FromQuery(Name = "party")]
    public string Party { get; set; }

    /// <summary>
    /// From party (uuid)
    /// </summary>
    [FromQuery(Name = "from")]
    public string From { get; set; }

    /// <summary>
    /// To party (uuid)
    /// </summary>
    [FromQuery(Name = "to")]
    public string To { get; set; }
}

/// <summary>
/// Base input dto for creating a new request
/// </summary>
public class CreateRequestInput
{
    /// <summary>
    /// Request connection
    /// </summary>
    public ConnectionRequestInputDto Connection { get; set; }
}

public class ConnectionRequestInputDto
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
/// Input for creating a new request for an access package
/// </summary>
public class CreatePackageRequestInput : CreateRequestInput
{
    /// <summary>
    /// Reference to the access package
    /// </summary>
    public PackageReferenceDto Package { get; set; }
}

/// <summary>
/// Input for creating a new request for a resource
/// </summary>
public class CreateResourceRequestInput : CreateRequestInput
{
    /// <summary>
    /// Reference to the resource
    /// </summary>
    public ResourceReferenceDto Resource { get; set; }
}

/// <summary>
/// Resource reference
/// </summary>
public class ResourceReferenceDto
{
    /// <summary>
    /// Identifying the resource
    /// </summary>
    public string ResourceId { get; set; }
}

/// <summary>
/// Access package reference
/// </summary>
public class PackageReferenceDto
{
    /// <summary>
    /// Urn identifying the package
    /// </summary>
    public string Urn { get; set; }
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
    /// Discriminator indicating the request type: "resource", "package", or "assignment"
    /// </summary>
    public string RequestType { get; set; }

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
    /// Link for the end user to confirm the request (change status from draft to pending)
    /// </summary>
    public string ConfirmLink { get; set; }

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
    /// Party that access is requested for
    /// </summary>
    public PartyEntityDto To { get; set; }
}

/// <summary>
/// Response dto for a resource request
/// </summary>
public class RequestResourceDto : RequestDto
{
    /// <summary>
    /// Resource that is requested
    /// </summary>
    public ResourceReferenceDto Resource { get; set; }
}

/// <summary>
/// Response dto for an access package request
/// </summary>
public class RequestPackageDto : RequestDto
{
    /// <summary>
    /// Access package that is requested
    /// </summary>
    public PackageReferenceDto Package { get; set; }
}

/// <summary>
/// Party entity reference
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
