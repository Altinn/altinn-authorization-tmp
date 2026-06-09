using System.ComponentModel.DataAnnotations.Schema;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Base;

/// <summary>
/// Resource queue base class
/// </summary>
[NotMapped]
public class BaseResourceQueue
{
    /// <summary>
    /// Identity
    /// </summary>
    public long Id { get; set; }
}
