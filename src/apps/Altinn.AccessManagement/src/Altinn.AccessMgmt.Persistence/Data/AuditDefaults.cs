﻿namespace Altinn.AccessMgmt.Persistence.Data;

/// <summary>
/// Default values for Audit system entities
/// </summary>
public static class AuditDefaults
{
    /// <summary>
    /// StaticDataIngest
    /// </summary>
    public static readonly Guid StaticDataIngest = Guid.Parse("3296007F-F9EA-4BD0-B6A6-C8462D54633A");

    /// <summary>
    /// RegisterImportSystem
    /// </summary>
    public static readonly Guid RegisterImportSystem = Guid.Parse("EFEC83FC-DEBA-4F09-8073-B4DD19D0B16B");

    /// <summary>
    /// EnduserApi
    /// </summary>
    public static readonly Guid EnduserApi = Guid.Parse("ED771364-42A8-4934-801E-B482ED20EC3E");

    /// <summary>
    /// Altinn2ClientImportSystem
    /// </summary>
    public static readonly Guid Altinn2ClientImportSystem = Guid.Parse("46CFA478-971F-446E-9BC1-AF57469361D0");
}
