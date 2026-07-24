using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.AccessManagement.Integration.Services.Interfaces;
using Altinn.AccessMgmt.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Integration.Clients;

/// <summary>
/// Client for getting Altinn roles from AltinnII SBL Bridge
/// </summary>
[ExcludeFromCodeCoverage]
public class Altinn2RightsClient : IAltinn2RightsClient
{
    private readonly SblBridgeSettings _sblBridgeSettings;
    private readonly HttpClient _client;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    private readonly IPlatformAuthorizationTokenProvider _sblTokenProvider;
    private readonly IFeatureManager _featureManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnRolesClient"/> class
    /// </summary>
    /// <param name="httpClient">HttpClient from default httpclientfactory</param>
    /// <param name="sblBridgeSettings">the sbl bridge settings</param>
    /// <param name="logger">the logger</param>
    /// <param name="sblTokenProvider">instance of authorization platform token provider</param>
    /// <param name="featureManager">the feature manager</param>
    public Altinn2RightsClient(HttpClient httpClient, IOptions<SblBridgeSettings> sblBridgeSettings, ILogger<AltinnRolesClient> logger, IPlatformAuthorizationTokenProvider sblTokenProvider, IFeatureManager featureManager)
    {
        _sblBridgeSettings = sblBridgeSettings.Value;
        _logger = logger;
        _client = httpClient;
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _serializerOptions.Converters.Add(new JsonStringEnumConverter());
        _sblTokenProvider = sblTokenProvider;
        _featureManager = featureManager;
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> ClearReporteeRights(int fromPartyId, int toPartyId, int toUserId = 0, CancellationToken cancellationToken = default)
    {
        if (await _featureManager.IsEnabledAsync(AccessMgmtFeatureFlags.DisableAltinn2CacheInvalidation))
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        UriBuilder endpoint = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}cache/api/clearreporteerights?reporteePartyId={fromPartyId}&coveredByPartyId={toPartyId}&coveredByUserId={toUserId}");
        return await _client.PutAsync(endpoint.Uri.ToString(), null, cancellationToken);
    }
}
