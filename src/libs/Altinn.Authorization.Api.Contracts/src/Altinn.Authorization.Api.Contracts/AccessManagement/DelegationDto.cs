using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorization.Api.Contracts.AccessManagement;

public class DelegationDto
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// PackageId
    /// </summary>
    [JsonPropertyName("packageId")]
    public Guid PackageId { get; set; }

    /// <summary>
    /// PackageId
    /// </summary>
    [JsonPropertyName("viaId")]
    public Guid ViaId { get; set; }

    /// <summary>
    /// FromId
    /// </summary>
    [JsonPropertyName("fromId")]
    public Guid FromId { get; set; }

    /// <summary>
    /// ToId
    /// </summary>
    [JsonPropertyName("toId")]
    public Guid ToId { get; set; }
}
