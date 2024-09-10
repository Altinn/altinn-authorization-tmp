using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.Platform.Authorization.Clients;
using Altinn.Platform.Authorization.Models;
using Altinn.Platform.Authorization.Models.AccessManagement;
using Altinn.Platform.Authorization.Services.Interface;
using Microsoft.Extensions.Logging;

namespace Altinn.Platform.Authorization.Services.Implementation
{
    /// <summary>
    /// Wrapper for the Altinn Access Management API
    /// </summary>
    public class AccessManagementWrapper : IAccessManagementWrapper
    {
        private readonly AccessManagementClient _client;
        private readonly ILogger<AccessManagementWrapper> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessManagementWrapper"/> class.
        /// </summary>
        public AccessManagementWrapper(ILogger<AccessManagementWrapper> logger, AccessManagementClient client)
        {
            _logger = logger;
            _client = client;
        }

        /// <summary>
        /// Endpoint to find all delegation changes for a given user, reportee and app/resource context
        /// </summary>
        /// <returns>Input parameter to the request</returns>
        public async Task<IEnumerable<DelegationChangeExternal>> GetAllDelegationChanges(DelegationChangeInput input)
        {
            try
            {
                var response = await _client.Client.SendAsync(new(HttpMethod.Post, new Uri(new Uri(_client.Settings.Value.ApiAccessManagementEndpoint), "policyinformation/getdelegationchanges"))
                {
                    Content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, MediaTypeNames.Application.Json)
                });

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<IEnumerable<DelegationChangeExternal>>();
                }

                var content = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(content == string.Empty ? $"received status code {response.StatusCode}" : content);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("AccessManagement // AccessManagementWrapper // GetAllDelegationChanges // Failed // Unexpected Exception // Unexpected HttpStatusCode: {statusCode}\n {responseContent}", ex.StatusCode, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("AccessManagement // AccessManagementWrapper // GetAllDelegationChanges // Failed // Unexpected Exception // Unexpected Message: {message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Endpoint to find all delegation changes for a given user, reportee and app/resource context
        /// </summary>
        /// <param name="actions">optional funvation pattern for modifying the request sent to Access Management API</param>
        /// <returns></returns>
        public async Task<IEnumerable<DelegationChangeExternal>> GetAllDelegationChanges(params Action<DelegationChangeInput>[] actions)
        {
            var input = new DelegationChangeInput()
            {
                Resource = new List<AttributeMatch>(),
            };

            actions.ToList().ForEach(action => action(input));
            return await GetAllDelegationChanges(input);
        }
    }
}
