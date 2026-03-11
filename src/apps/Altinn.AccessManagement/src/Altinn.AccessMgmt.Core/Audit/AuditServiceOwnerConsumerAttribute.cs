namespace Altinn.AccessMgmt.Core.Audit;

/// <summary>
/// Attribute to decorate for service owner consumer
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AuditServiceOwnerConsumerAttribute : Attribute
{
    /// <summary>
    /// Which system that initiates the request.
    /// </summary>
    public string System { get; set; }
}
