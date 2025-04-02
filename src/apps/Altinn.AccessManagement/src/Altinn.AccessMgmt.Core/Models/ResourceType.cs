using System.Security.Cryptography;
using System.Text;

namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// ResourceType
/// </summary>
public class ResourceType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceType"/> class.
    /// </summary>
    public ResourceType()
    {
        Id = Guid.CreateVersion7();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceType"/> class.
    /// E.g. MD5.HashData(Encoding.UTF8.GetBytes("some-unique-identity"));
    /// </summary>
    /// <param name="keyHash">Hash to be used when generation identity</param>
    public ResourceType(byte[] keyHash)
    {
        Id = new Guid(keyHash);
    }

    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }
}
