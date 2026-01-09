using Altinn.AccessMgmt.PersistenceEF.Models.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models;

/// <summary>
/// Extended delegation
/// </summary>
public class ErrorQueue : BaseErrorQueue
{
    /// <summary>
    /// The assosiated delegationChangeId (app/resource/instance)
    /// </summary>
    public long DelegationChangeId { get; set; }

    /// <summary>
    /// The origion of the change (app/resource/instance)
    /// </summary>
    public string OriginType { get; set; }

    /// <summary>
    /// Json containing the instance/app/resource
    /// </summary>
    public string ErrorItem { get; set; }

    /// <summary>
    /// Logs the error message associated with the failed processing attempt.
    /// </summary>
    public string ErrorMessage { get; set; }
}
