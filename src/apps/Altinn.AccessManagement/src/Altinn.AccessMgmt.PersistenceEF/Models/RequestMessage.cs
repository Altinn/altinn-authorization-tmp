using Altinn.AccessMgmt.PersistenceEF.Models.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models;

public class RequestMessage : BaseRequestMessage
{
    /// <summary>
    /// Request
    /// </summary>
    public Request Request { get; set; }

    /// <summary>
    /// Author of the message
    /// </summary>
    public Entity Author { get; set; }
}
