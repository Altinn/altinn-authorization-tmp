using Altinn.AccessMgmt.PersistenceEF.Models.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models;

/// <summary>
/// Resources in request
/// </summary>
public class RequestResourceElement : BaseRequestResourceElement
{
    /// <summary>
    /// Request
    /// </summary>
    public Request Request { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    public RequestStatus Status { get; set; }

    /// <summary>
    /// Resource requested
    /// </summary>
    public Resource Resource { get; set; }

    /// <summary>
    /// Resource requested
    /// </summary>
    public ResourceElement Element { get; set; }
}
