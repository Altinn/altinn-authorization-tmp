namespace Altinn.AccessManagement.Core.Models.Connection;

/// <summary>
/// Connection input core model for business logic
/// </summary>
public class ConnectionInput
{
    /// <summary>
    /// The party making the request
    /// </summary>
    public required string Party { get; set; }

    /// <summary>
    /// The party the connection is from
    /// </summary>
    public required string From { get; set; }

    /// <summary>
    /// The party the connection is to
    /// </summary>
    public required string To { get; set; }
}