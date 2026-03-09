using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorization.Api.Contracts.AccessManagement;

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
