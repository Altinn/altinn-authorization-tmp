using System.ComponentModel.DataAnnotations.Schema;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Base;

/// <summary>
/// Define the types of Providers
/// </summary>
[NotMapped]
public class BaseProviderType
{
    /// <summary>
    /// Provider type identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Provider type name
    /// </summary>
    public string Name { get; set; }
}
