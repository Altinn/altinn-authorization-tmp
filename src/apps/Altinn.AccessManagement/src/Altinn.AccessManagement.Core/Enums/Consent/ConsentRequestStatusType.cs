using System.Runtime.Serialization;

namespace Altinn.AccessManagement.Core.Enums.Consent
{
    /// <summary>
    /// Enum for the status of a consent request
    /// </summary>
    public enum ConsentRequestStatusType : int
    {
        Created = 0,
        Rejected = 1,
        Approved = 2
    }
}
