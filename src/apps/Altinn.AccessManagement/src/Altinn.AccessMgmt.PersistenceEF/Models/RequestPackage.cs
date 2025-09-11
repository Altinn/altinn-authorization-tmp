using Altinn.AccessMgmt.PersistenceEF.Models.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models;

/// <summary>
/// Packages in request
/// </summary>
public class RequestPackage : BaseRequestPackage
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
    /// Package requested
    /// </summary>
    public Package Package { get; set; }
}
