namespace Altinn.Authorization.Api.Contracts.AccessManagement.Common;

/// <summary>
/// Detail information for delegation results
/// </summary>
public class DetailExternal
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Parameters { get; set; } = [];
}