using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Party;
using Altinn.Authorization.Core.Models.Register;
using Altinn.Authorization.ProblemDetails;
using Altinn.Platform.Register.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Altinn.AccessManagement.Core.Services
{
    /// <summary>
    /// Service for handling consent
    /// </summary>
    /// <remarks>
    /// Service responsible for consent functionality
    /// </remarks>
    public class ConsentService(IConsentRepository consentRepository, IPartiesClient partiesClient, ISingleRightsService singleRightsService, IResourceRegistryClient resourceRegistryClient, IAMPartyService ampartyService, IMemoryCache memoryCache) : IConsent
    {
        private readonly IConsentRepository _consentRepository = consentRepository;
        private readonly IPartiesClient _partiesClient = partiesClient;
        private readonly ISingleRightsService _singleRightsService = singleRightsService;
        private readonly IResourceRegistryClient _resourceRegistryClient = resourceRegistryClient;
        private readonly IAMPartyService _ampartyService = ampartyService;
        private readonly IMemoryCache _memoryCache = memoryCache;

        private const string _consentRequestStatus = "Status";

        /// <inheritdoc/>
        public async Task<Result<ConsentRequestDetails>> CreateRequest(ConsentRequest consentRequest, ConsentPartyUrn performedByParty, CancellationToken cancellationToken)
        {
            Result<ConsentRequest> result = await ValidateAndSetInternalIdentifiers(consentRequest, cancellationToken);

            if (result.IsProblem)
            {
                return result.Problem;
            }

            performedByParty = await MapFromExternalIdenity(performedByParty, cancellationToken);

            ConsentRequestDetails requestDetails = await _consentRepository.CreateRequest(result.Value, performedByParty, cancellationToken);
            requestDetails.From = consentRequest.From;
            requestDetails.To = consentRequest.To;
            foreach (ConsentRequestEvent consentRequestEvent in requestDetails.ConsentRequestEvents)
            {
                consentRequestEvent.PerformedBy = await MapToExternalIdenity(consentRequestEvent.PerformedBy, cancellationToken);
            }

            return requestDetails;
        }

        /// <inheritdoc/>
        public async Task<Result<ConsentRequestDetails>> RejectRequest(Guid id, Guid performedByParty, CancellationToken cancellationToken)
        {
            ValidationErrorBuilder errors = default;
            ConsentRequestDetails details = await _consentRepository.GetRequest(id, cancellationToken);
            if (details.ConsentRequestStatus == ConsentRequestStatusType.Rejected)
            {
                await SetExternalIdentities(details, cancellationToken);
                return details;
            }

            if (details.ConsentRequestStatus != ConsentRequestStatusType.Created)
            {
                errors.Add(ValidationErrors.ConsentCantBeRejected, _consentRequestStatus);
            }

            if (errors.TryBuild(out var beforeErrorREsult))
            {
                return beforeErrorREsult;
            }

            try
            {
                await _consentRepository.RejectConsentRequest(id, performedByParty, cancellationToken);
            }
            catch (Exception)
            {
                await _consentRepository.GetRequest(id, cancellationToken);

                if (details.ConsentRequestStatus == ConsentRequestStatusType.Rejected)
                {
                    await SetExternalIdentities(details, cancellationToken);
                    return details;
                }

                if (details.ConsentRequestStatus != ConsentRequestStatusType.Created)
                {
                    errors.Add(ValidationErrors.ConsentCantBeRejected, _consentRequestStatus);
                    if (errors.TryBuild(out var errorResult))
                    {
                        return errorResult;
                    }
                }

                throw;
            }

            ConsentRequestDetails updated = await _consentRepository.GetRequest(id, cancellationToken);
            await SetExternalIdentities(updated, cancellationToken);

            return updated;
        }

        /// <inheritdoc/>
        public async Task<Result<Consent>> GetConsent(Guid id, ConsentPartyUrn from, ConsentPartyUrn to, CancellationToken cancellationToken)
        {
            ValidationErrorBuilder errors = default;
            ConsentRequestDetails consentRequest = await _consentRepository.GetRequest(id, cancellationToken);

            // Map from external to internal identies 
            from = await MapFromExternalIdenity(from, cancellationToken);
            to = await MapFromExternalIdenity(to, cancellationToken);

            if (consentRequest == null)
            {
                errors.Add(ValidationErrors.ConsentNotFound, "From");

                if (errors.TryBuild(out var errorResultStart))
                {
                    return errorResultStart;
                }
            }
            else
            {
                if (!to.Equals(consentRequest.To))
                {
                    errors.Add(ValidationErrors.ConsentNotFound, "To");
                }

                if (!from.Equals(consentRequest.From))
                {
                    errors.Add(ValidationErrors.MissMatchConsentParty, "From");
                }

                if (consentRequest.ValidTo < DateTime.UtcNow)
                {
                    errors.Add(ValidationErrors.ConsentExpired, "ValidTo");
                }

                if (consentRequest.ConsentRequestStatus == ConsentRequestStatusType.Created)
                {
                    errors.Add(ValidationErrors.ConsentNotAccepted, _consentRequestStatus);
                }
                else if (consentRequest.ConsentRequestStatus == ConsentRequestStatusType.Revoked)
                {
                    errors.Add(ValidationErrors.ConsentRevoked, _consentRequestStatus);
                }
                else if (consentRequest.ConsentRequestStatus != ConsentRequestStatusType.Accepted)
                {
                    errors.Add(ValidationErrors.ConsentNotAccepted, _consentRequestStatus);
                }
            }

            if (errors.TryBuild(out var errorResult))
            {
                return errorResult;
            }

            Consent consent = new Consent()
            {
                Id = consentRequest.Id,
                From = await MapToExternalIdenity(consentRequest.From, cancellationToken),
                To = await MapToExternalIdenity(consentRequest.To, cancellationToken),
                ValidTo = consentRequest.ValidTo,
                ConsentRights = consentRequest.ConsentRights
            };

            return consent;
        }

        /// <inheritdoc/>
        public async Task<ConsentRequestDetails> GetRequest(Guid id, Guid userId, CancellationToken cancellationToken)
        {
            ConsentRequestDetails details = await _consentRepository.GetRequest(id, cancellationToken);
            bool isAuthorized = await AuthorizeUserForConsentRequest(userId, details, cancellationToken);
            details.To = await MapToExternalIdenity(details.To, cancellationToken);
            details.From = await MapToExternalIdenity(details.From, cancellationToken);
            foreach (ConsentRequestEvent consentRequestEvent in details.ConsentRequestEvents)
            {
                consentRequestEvent.PerformedBy = await MapToExternalIdenity(consentRequestEvent.PerformedBy, cancellationToken);
            }

            return details;
        }

        /// <inheritdoc/>
        public async Task<Result<ConsentRequestDetails>> AcceptRequest(Guid id, Guid performedByParty, CancellationToken cancellationToken)
        {
            ValidationErrorBuilder errors = default;
            ConsentRequestDetails details = await _consentRepository.GetRequest(id, cancellationToken);
            if (details.ConsentRequestStatus == ConsentRequestStatusType.Accepted)
            {
                await SetExternalIdentities(details, cancellationToken);
                return details;
            }

            if (details.ConsentRequestStatus != ConsentRequestStatusType.Created)
            {
                errors.Add(ValidationErrors.ConsentCantBeAccepted, _consentRequestStatus);
            }

            if (errors.TryBuild(out var beforeErrorREsult))
            {
                return beforeErrorREsult;
            }

            try
            {
                await _consentRepository.AcceptConsentRequest(id, performedByParty, cancellationToken);
            }
            catch (Exception)
            {
                details = await _consentRepository.GetRequest(id, cancellationToken);

                if (details.ConsentRequestStatus == ConsentRequestStatusType.Accepted)
                {
                    await SetExternalIdentities(details, cancellationToken);
                    return details;
                }

                if (details.ConsentRequestStatus != ConsentRequestStatusType.Created)
                {
                    errors.Add(ValidationErrors.ConsentCantBeAccepted, _consentRequestStatus);
                    if (errors.TryBuild(out var errorResult))
                    {
                        return errorResult;
                    }
                }

                throw;
            }

            ConsentRequestDetails updated = await _consentRepository.GetRequest(id, cancellationToken);
            await SetExternalIdentities(updated, cancellationToken);
            return updated;
        }

        /// <inheritdoc/>
        public async Task<Result<ConsentRequestDetails>> RevokeConsent(Guid id, Guid performedByParty, CancellationToken cancellationToken = default)
        {
            ValidationErrorBuilder errors = default;
            ConsentRequestDetails details = await _consentRepository.GetRequest(id, cancellationToken);
            if (details.ConsentRequestStatus == ConsentRequestStatusType.Revoked)
            {
                await SetExternalIdentities(details, cancellationToken);
                return details;
            }

            if (details.ConsentRequestStatus != ConsentRequestStatusType.Accepted)
            {
                errors.Add(ValidationErrors.ConsentCantBeRevoked, _consentRequestStatus);
            }

            if (errors.TryBuild(out var beforeErrorREsult))
            {
                return beforeErrorREsult;
            }

            try
            {
                await _consentRepository.Revoke(id, performedByParty, cancellationToken);
            }
            catch (Exception)
            {
                details = await _consentRepository.GetRequest(id, cancellationToken);

                if (details.ConsentRequestStatus == ConsentRequestStatusType.Revoked)
                {
                    await SetExternalIdentities(details, cancellationToken);
                    return details;
                }

                if (details.ConsentRequestStatus != ConsentRequestStatusType.Accepted)
                {
                    errors.Add(ValidationErrors.ConsentCantBeAccepted, _consentRequestStatus);
                    if (errors.TryBuild(out var errorResult))
                    {
                        return errorResult;
                    }
                }

                throw;
            }

            ConsentRequestDetails updated = await _consentRepository.GetRequest(id, cancellationToken);
            await SetExternalIdentities(updated, cancellationToken);
            return updated;
        }

        private async Task SetExternalIdentities(ConsentRequestDetails details, CancellationToken cancellationToken)
        {
            details.From = await MapToExternalIdenity(details.From, cancellationToken);
            details.To = await MapToExternalIdenity(details.To, cancellationToken);
            foreach (ConsentRequestEvent consentRequestEvent in details.ConsentRequestEvents)
            {
                consentRequestEvent.PerformedBy = await MapToExternalIdenity(consentRequestEvent.PerformedBy, cancellationToken);
            }
        }

        private async Task<ConsentPartyUrn> MapFromExternalIdenity(ConsentPartyUrn consentPartyUrn, CancellationToken cancellationToken)
        {
            if (consentPartyUrn.IsPersonId(out PersonIdentifier personIdentifier))
            {
                return await GetInternalIdentifier(personIdentifier, cancellationToken);
            }
            else if (consentPartyUrn.IsOrganizationId(out OrganizationNumber organizationNumber))
            {
                return await GetInternalIdentifier(organizationNumber, cancellationToken);
            }

            return consentPartyUrn;
        }

        private async Task<ConsentPartyUrn> MapToExternalIdenity(ConsentPartyUrn consentPartyUrn, CancellationToken cancellationToken)
        {
            if (consentPartyUrn.IsPartyUuid(out Guid partyUuid))
            {
                if (_memoryCache.TryGetValue(partyUuid, out ConsentPartyUrn consentPartyUrnCache))
                {
                    return consentPartyUrnCache;
                }
                else
                {
                    ConsentPartyUrn consentPartyUrnFromRegister = await GetExternalIdentifier(partyUuid, cancellationToken);
                    _memoryCache.Set(partyUuid, consentPartyUrnFromRegister, TimeSpan.FromMinutes(5));
                    return consentPartyUrnFromRegister;
                }
            }

            return consentPartyUrn;
        }

        private async Task<ConsentPartyUrn> GetExternalIdentifier(Guid guid, CancellationToken cancellationToken)
        {
            MinimalParty party = await _ampartyService.GetByUid(guid, cancellationToken);

            if (party == null)
            {
                throw new ArgumentException($"Party with guid {guid} not found");
            }

            if (!string.IsNullOrEmpty(party.PersonId))
            {
                return ConsentPartyUrn.PersonId.Create(PersonIdentifier.Parse(party.PersonId));
            }

            if (!string.IsNullOrEmpty(party.OrganizationId))
            {
                return ConsentPartyUrn.OrganizationId.Create(OrganizationNumber.Parse(party.OrganizationId));
            }

            throw new ArgumentException($"Party with guid {guid} is not valid consent party");
        }

        private async Task<ConsentPartyUrn> GetInternalIdentifier(OrganizationNumber organizationNumber, CancellationToken cancellationToken)
        {
            MinimalParty party = await _ampartyService.GetByOrgNo(organizationNumber.ToString(), cancellationToken);
            if (party == null)
            {
                return null;
            }

            return ConsentPartyUrn.PartyUuid.Create(party.PartyUuid);
        }

        private async Task<ConsentPartyUrn> GetInternalIdentifier(PersonIdentifier personIdentifier, CancellationToken cancellationToken)
        {
            MinimalParty party = await _ampartyService.GetByPersonNo(personIdentifier.ToString(), cancellationToken);
            if (party == null)
            {
                return null;
            }

            return ConsentPartyUrn.PartyUuid.Create(party.PartyUuid);
        }

        /// <summary>
        /// This method iterates throug the consent request and verifies that user is allowe to delegate all rights requested in consent
        /// Currently no sub resources is supported. Ignores sub resources in response.
        /// TODO: Verify when we have new delegation check with support for 
        /// </summary>
        private async Task<bool> AuthorizeUserForConsentRequest(Guid userUuid, ConsentRequestDetails consentRequest, CancellationToken cancellationToken)
        {
            Guid fromParty = consentRequest.From.IsPartyUuid(out Guid from) ? from : Guid.Empty;
            List<Party> parties = await _partiesClient.GetPartiesAsync(new List<Guid> { fromParty }, cancellationToken: cancellationToken);
            Party party = parties.First();

            int userID = await GetUserIdForParty(userUuid);

            foreach (ConsentRight consentRight in consentRequest.ConsentRights)
            {
                RightsDelegationCheckRequest rightsDelegationCheckRequest = new()
                {
                    From = [new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, Value = party.PartyId.ToString() }]
                };

                foreach (ConsentResourceAttribute resource in consentRight.Resource)
                {
                    rightsDelegationCheckRequest.Resource = [new AttributeMatch { Id = resource.Type, Value = resource.Value }];
                }

                DelegationCheckResponse response = await _singleRightsService.RightsDelegationCheck(userID, 3, rightsDelegationCheckRequest);
             
                if (response.RightDelegationCheckResults != null)
                {
                    foreach (string action in consentRight.Action)
                    {
                        bool actionMatch = false;
                        foreach (RightDelegationCheckResult result in response.RightDelegationCheckResults)
                        {
                            if (result.Action.Equals(action) && result.Status.Equals(DelegableStatus.Delegable))
                            {
                                actionMatch = true;
                                break;
                            }
                        }

                        if (!actionMatch)
                        {
                            return false;
                        }
                    }

                    // We only support one action per consent  per now
                    return true;
                }
            }

            // Temporary return true until we have the actual response
            return true;
        }

        /// <summary>
        /// Validates and sets internal identifiers for the consent request
        /// - Validates that the from and to party is valid
        /// - Validates that resources requested in consent is valid
        /// - Validates that valid to time is valid
        /// </summary>
        private async Task<Result<ConsentRequest>> ValidateAndSetInternalIdentifiers(ConsentRequest consentRequest, CancellationToken cancelactionToken)
        {
            ValidationErrorBuilder errors = default;
            ConsentPartyUrn from = await MapFromExternalIdenity(consentRequest.From, cancelactionToken);
            if (from == null)
            {
                if (consentRequest.From.IsOrganizationId(out _))
                {
                    errors.Add(ValidationErrors.InvalidOrganizationIdentifier, "From");
                }
                else if (consentRequest.From.IsPersonId(out _))
                {
                    errors.Add(ValidationErrors.InvalidPersonIdentifier, "From");
                }
            }
            else
            {
                consentRequest.From = from;
            }

            ConsentPartyUrn to = await MapFromExternalIdenity(consentRequest.To, cancelactionToken);
            if (to == null)
            {
                if (consentRequest.To.IsOrganizationId(out _))
                {
                    errors.Add(ValidationErrors.InvalidOrganizationIdentifier, "To");
                }
                else if (consentRequest.To.IsPersonId(out _))
                {
                    errors.Add(ValidationErrors.InvalidPersonIdentifier, "To");
                }
            }
            else
            {
                consentRequest.To = to;
            }

            if (consentRequest.ValidTo < DateTime.UtcNow)
            {
                errors.Add(ValidationErrors.InvalidValidToTime, "ValidTo");
            }

            if (consentRequest.ConsentRights == null || consentRequest.ConsentRights.Count == 0)
            {
                errors.Add(ValidationErrors.MissingConsentRight, "Resource");
            }
            else
            {
                for (int rightIndex = 0; rightIndex < consentRequest.ConsentRights.Count; rightIndex++)
                {
                    ConsentRight consentRight = consentRequest.ConsentRights[rightIndex];

                    if (consentRight.Action == null || consentRight.Action.Count == 0)
                    {
                        errors.Add(ValidationErrors.MissingAction, $"/consentRight/{rightIndex}/action");
                    }

                    if (consentRight.Resource == null || consentRight.Resource.Count == 0 || consentRight.Resource.Count > 1)
                    {
                        errors.Add(ValidationErrors.InvalidResource, "Resource");
                    }
                    else
                    {
                        ServiceResource resourceDetails = await _resourceRegistryClient.GetResource(consentRight.Resource[0].Value, cancelactionToken);
                        if (resourceDetails == null)
                        {   
                            errors.Add(ValidationErrors.InvalidConsentResource, "Resource");
                        }
                        else if (!resourceDetails.ResourceType.Equals(ResourceType.Consentresource))
                        {
                            errors.Add(ValidationErrors.InvalidConsentResource, "Resource");
                        }
                        else
                        {
                            if (consentRight.MetaData != null && consentRight.MetaData.Count > 0)
                            {
                                foreach (KeyValuePair<string, string> metaData in consentRight.MetaData)
                                {
                                    if (resourceDetails.ConsentMetadata == null || !resourceDetails.ConsentMetadata.ContainsKey(metaData.Key.ToLower()))
                                    {
                                        errors.Add(ValidationErrors.UnknownConsentMetadata, $"/consentRight/{rightIndex}/Metadata/{metaData.Key.ToLower()}");
                                    }

                                    if (string.IsNullOrEmpty(metaData.Value))
                                    {
                                        errors.Add(ValidationErrors.MissingMetadataValue, $"/consentRight/{rightIndex}/Metadata");
                                    }
                                }
                            }

                            if (resourceDetails.ConsentMetadata != null)
                            {
                                foreach (KeyValuePair<string, ConsentMetadata> consentMetadata in resourceDetails.ConsentMetadata)
                                {
                                    if (consentRight.MetaData == null || !consentRight.MetaData.ContainsKey(consentMetadata.Key))
                                    {
                                        errors.Add(ValidationErrors.MissingMetadata, $"/consentRight/{rightIndex}/Metadata/{consentMetadata.Key}");
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (errors.TryBuild(out var errorResult))
            {
                return errorResult;
            }

            return consentRequest;
        }

        private async Task<int> GetUserIdForParty(Guid partyId)
        {
            return 20001337;

            //List<Party> parties = await _partiesClient.GetPartiesAsync(new List<Guid> { partyId });
            //Party party = parties.First();
            //return party.PartyId;
        }
    }
}
