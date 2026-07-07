using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Models.IdPortenAuthorization;
using Altinn.AccessManagement.Core.Models.Party;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.ProblemDetails;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Core.Services
{
    public class IdPortenAuthorizationService : IIdPortenAuthorizationService
    {
        private readonly ILogger<IdPortenAuthorizationService> _logger;
        private readonly IAMPartyService _ampartyService;
        private readonly GeneralSettings _generalSettings;
        private readonly IIdPortenAuthorizationClient _idPortenAuthorizationClient;

        public IdPortenAuthorizationService(
            ILogger<IdPortenAuthorizationService> logger,
            IAMPartyService ampartyService,
            IOptions<GeneralSettings> generalSettings,
            IIdPortenAuthorizationClient idPortenAuthorizationClient)
        {
            _logger = logger;
            _ampartyService = ampartyService;
            _generalSettings = generalSettings.Value;
            _idPortenAuthorizationClient = idPortenAuthorizationClient;
        }

        public async Task<Result<List<IdPortenAuthorization>>> GetIdPortenAuthorizations(Guid partyUuid, CancellationToken cancellationToken)
        {
            // look up ssn from partyUuid
            string ssn = await GetSsnFromPartyUuid(partyUuid, cancellationToken);

            if (string.IsNullOrEmpty(ssn))
            {
                return Problems.SsnNotFound;
            }

            // look up authorizations from IdPorten using ssn
            return await _idPortenAuthorizationClient.GetIdPortenAuthorizations(ssn, cancellationToken);
        }

        public async Task<Result<bool>> DeleteIdPortenAuthorization(Guid partyUuid, string id, CancellationToken cancellationToken)
        {
            // look up ssn from partyUuid
            string ssn = await GetSsnFromPartyUuid(partyUuid, cancellationToken);
            
            if (string.IsNullOrEmpty(ssn))
            {
                return Problems.SsnNotFound;
            }

            // delete authorization from IdPorten using ssn and id
            return await _idPortenAuthorizationClient.DeleteIdPortenAuthorization(ssn, id, cancellationToken);
        }

        private async Task<string?> GetSsnFromPartyUuid(Guid partyUuid, CancellationToken cancellationToken)
        {
            // look up ssn from partyUuid
            MinimalParty party = await _ampartyService.GetByUid(partyUuid, cancellationToken);
            return party?.PersonId;
        } 
    }
}
