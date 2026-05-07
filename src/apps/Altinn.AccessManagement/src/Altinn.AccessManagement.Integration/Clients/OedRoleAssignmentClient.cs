using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Integration.Clients;

/// <summary>
/// Implementation of <see cref="IOedRoleAssignmentService"/> that calls the external OED Authz API
/// to retrieve OED (Digitalt dødsbo) role assignments between a deceased party and an inheriting party.
/// </summary>
public class OedRoleAssignmentClient : IOedRoleAssignmentService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;
    private readonly ILogger<OedRoleAssignmentClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OedRoleAssignmentClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client (configured via DI with base address and auth).</param>
    /// <param name="logger">The logger.</param>
    public OedRoleAssignmentClient(HttpClient httpClient, ILogger<OedRoleAssignmentClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetOedRoleCodes(string fromPersonId, string toPersonId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new OedRoleAssignmentRequest { From = fromPersonId, To = toPersonId };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync("api/v1/pip", content, cancellationToken);
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = JsonSerializer.Deserialize<OedRoleAssignmentsResponse>(responseContent, SerializerOptions);
                return result?.RoleAssignments?
                    .Select(r => r.OedRoleCode)
                    .Where(code => !string.IsNullOrEmpty(code))
                    .ToList() ?? [];
            }

            _logger.LogError("OedRoleAssignmentClient // GetOedRoleCodes // Unexpected HttpStatusCode: {StatusCode}\n {ResponseContent}", response.StatusCode, responseContent);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OedRoleAssignmentClient // GetOedRoleCodes // Failed");
            throw;
        }
    }

    private sealed class OedRoleAssignmentRequest
    {
        public string From { get; set; }

        public string To { get; set; }
    }

    private sealed class OedRoleAssignmentsResponse
    {
        public List<OedRoleAssignmentItem> RoleAssignments { get; set; }
    }

    private sealed class OedRoleAssignmentItem
    {
        [JsonPropertyName("urn:digitaltdodsbo:rolecode")]
        public string OedRoleCode { get; set; }

        public string From { get; set; }

        public string To { get; set; }
    }
}
