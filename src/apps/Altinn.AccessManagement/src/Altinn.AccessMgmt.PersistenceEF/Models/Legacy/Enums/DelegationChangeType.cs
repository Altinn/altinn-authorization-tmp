using NpgsqlTypes;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Legacy.Enums;

public enum DelegationChangeType
{
    /// <summary>
    /// Undefined default value
    /// </summary>
    // ReSharper disable UnusedMember.Global
    [PgName("")]
    Undefined = 0,

    /// <summary>
    /// Grant event
    /// </summary>
    [PgName("grant")]
    Grant = 1,

    /// <summary>
    /// Revoke event
    /// </summary>
    [PgName("revoke")]
    Revoke = 2,

    /// <summary>
    /// Revoke last right event
    /// </summary>
    [PgName("revoke_last")]
    RevokeLast = 3
}
