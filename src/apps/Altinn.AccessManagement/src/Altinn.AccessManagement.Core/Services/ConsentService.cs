using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Party;
using Altinn.Authorization.Core.Models.Register;
using Altinn.Authorization.ProblemDetails;
using Altinn.Platform.Profile.Models;
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
    public class ConsentService(IConsentRepository consentRepository, IPartiesClient partiesClient, ISingleRightsService singleRightsService, 
        IResourceRegistryClient resourceRegistryClient, IAMPartyService ampartyService, IMemoryCache memoryCache, IProfileClient profileClient, TimeProvider timeProvider) : IConsent
    {
        private readonly IConsentRepository _consentRepository = consentRepository;
        private readonly IPartiesClient _partiesClient = partiesClient;
        private readonly ISingleRightsService _singleRightsService = singleRightsService;
        private readonly IResourceRegistryClient _resourceRegistryClient = resourceRegistryClient;
        private readonly IAMPartyService _ampartyService = ampartyService;
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly IProfileClient _profileClient = profileClient;
        private readonly TimeProvider _timeProvider = timeProvider;

        private const string _consentRequestStatus = "Status";
        private const string ResourceParam = "Resource";

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
        public async Task<Result<ConsentRequestDetails>> RejectRequest(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken)
        {
            ValidationErrorBuilder errors = default;
            ConsentRequestDetails details = await _consentRepository.GetRequest(consentRequestId, cancellationToken);
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
                await _consentRepository.RejectConsentRequest(consentRequestId, performedByParty, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                await _consentRepository.GetRequest(consentRequestId, cancellationToken);

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

            ConsentRequestDetails updated = await _consentRepository.GetRequest(consentRequestId, cancellationToken);
            await SetExternalIdentities(updated, cancellationToken);

            return updated;
        }

        /// <inheritdoc/>
        public async Task<Result<Consent>> GetConsent(Guid consentRequestId, ConsentPartyUrn from, ConsentPartyUrn to, CancellationToken cancellationToken)
        {
            MultipleProblemBuilder errors = default;
            ConsentRequestDetails consentRequest = await _consentRepository.GetRequest(consentRequestId, cancellationToken);

            // Map from external to internal identies 
            from = await MapFromExternalIdenity(from, cancellationToken);
            to = await MapFromExternalIdenity(to, cancellationToken);

            if (consentRequest == null)
            {
                return Problems.ConsentNotFound;    
            }
            else
            {
                errors = ValidateGetConsentRequest(from, to, errors, consentRequest);

                if (errors.TryBuild(out var errorResult))
                {
                    return errorResult;
                }

                Consent consent = new()
                {
                    Id = consentRequest.Id,
                    From = await MapToExternalIdenity(consentRequest.From, cancellationToken),
                    To = await MapToExternalIdenity(consentRequest.To, cancellationToken),
                    ValidTo = consentRequest.ValidTo,
                    ConsentRights = consentRequest.ConsentRights
                };

                return consent;
            }
        }

        public async Task<Result<Consent>> GetConsent(Guid consentRequestId, CancellationToken cancellationToken)
        {
            ValidationErrorBuilder errors = default;
            ConsentRequestDetails consentRequest = await _consentRepository.GetRequest(consentRequestId, cancellationToken);

            // TODO Authorize user for consent request
            if (consentRequest == null)
            {
                errors.Add(ValidationErrors.ConsentNotFound, "From");

                if (errors.TryBuild(out var errorResultStart))
                {
                    return errorResultStart;
                }

                // Should not be possible to get here
                throw new ArgumentException($"Consent request with id {consentRequestId} not found");
            }
            else
            {
                Consent consent = new()
                {
                    Id = consentRequest.Id,
                    From = await MapToExternalIdenity(consentRequest.From, cancellationToken),
                    To = await MapToExternalIdenity(consentRequest.To, cancellationToken),
                    ValidTo = consentRequest.ValidTo,
                    ConsentRights = consentRequest.ConsentRights
                };

                consent.Context = await _consentRepository.GetConsentContext(consentRequestId, cancellationToken);

                return consent;
            }
        }

        private MultipleProblemBuilder ValidateGetConsentRequest(ConsentPartyUrn from, ConsentPartyUrn to, MultipleProblemBuilder problemsBUilders, ConsentRequestDetails consentRequest)
        {
            if (!to.Equals(consentRequest.To))
            {
                problemsBUilders.Add(Problems.ConsentNotFound);
            }

            if (!from.Equals(consentRequest.From))
            {
                problemsBUilders.Add(Problems.MissMatchConsentParty);
            }

            if (consentRequest.ValidTo < _timeProvider.GetUtcNow())
            {
                problemsBUilders.Add(Problems.ConsentExpired);
            }

            if (consentRequest.ConsentRequestStatus == ConsentRequestStatusType.Created)
            {
                problemsBUilders.Add(Problems.ConsentNotAccepted);
            }
            else if (consentRequest.ConsentRequestStatus == ConsentRequestStatusType.Revoked)
            {
                problemsBUilders.Add(Problems.ConsentRevoked);
            }
            else if (consentRequest.ConsentRequestStatus != ConsentRequestStatusType.Accepted)
            {
                problemsBUilders.Add(Problems.ConsentNotAccepted);
            }

            return problemsBUilders;
        }

        /// <inheritdoc/>
        public async Task<Result<ConsentRequestDetails>> GetRequest(Guid consentRequestId, ConsentPartyUrn performedByParty, CancellationToken cancellationToken)
        {
            ConsentRequestDetails details = await _consentRepository.GetRequest(consentRequestId, cancellationToken);
            if (details == null)
            {
                return Problems.ConsentNotFound;
            }

            details.To = await MapToExternalIdenity(details.To, cancellationToken);

            if (performedByParty.IsOrganizationId(out Authorization.Core.Models.Register.OrganizationNumber organizationNumber))
            {
                if (details.To.IsOrganizationId(out Authorization.Core.Models.Register.OrganizationNumber toOrganizationNumber))
                {
                    if (!toOrganizationNumber.ToString().Equals(organizationNumber.ToString()))
                    {
                        return Problems.NotAuthorizedForConsentRequest;
                    }
                }
                else
                {
                    return Problems.NotAuthorizedForConsentRequest;
                }
            }
            else if (performedByParty.IsPartyUuid(out Guid partyUuid))
            {
                bool isAuthorized = await AuthorizeUserForConsentRequest(partyUuid, details, cancellationToken);
                if (!isAuthorized)
                {
                    return Problems.NotAuthorizedForConsentRequest;
                }
            }

            details.From = await MapToExternalIdenity(details.From, cancellationToken);
            foreach (ConsentRequestEvent consentRequestEvent in details.ConsentRequestEvents)
            {
                consentRequestEvent.PerformedBy = await MapToExternalIdenity(consentRequestEvent.PerformedBy, cancellationToken);
            }

            return details;
        }

        /// <inheritdoc/>
        public async Task<Result<ConsentRequestDetails>> AcceptRequest(Guid consentRequestId, Guid performedByParty, ConsentContext context, CancellationToken cancellationToken)
        {
            ConsentRequestDetails details = await _consentRepository.GetRequest(consentRequestId, cancellationToken);

            if (details == null)
            {
                return Problems.ConsentNotFound;
            }

            bool isAuthorized = await AuthorizeUserForConsentRequest(performedByParty, details, cancellationToken);

            if (!isAuthorized)
            {
                return Problems.NotAuthorizedForConsentRequest;
            }

            if (details.ConsentRequestStatus == ConsentRequestStatusType.Accepted)
            {
                await SetExternalIdentities(details, cancellationToken);
                return details;
            }

            if (details.ConsentRequestStatus != ConsentRequestStatusType.Created)
            {
                return Problems.ConsentCantBeAccepted;
            }

            ValidationErrorBuilder errors = default;
            ValidateContext(details, context, errors);

            if (errors.TryBuild(out var beforeErrorREsult))
            {
                return beforeErrorREsult;
            }

            try
            {
                await _consentRepository.AcceptConsentRequest(consentRequestId, performedByParty, context, cancellationToken);
            }
            catch (Exception)
            {
                details = await _consentRepository.GetRequest(consentRequestId, cancellationToken);

                if (details.ConsentRequestStatus == ConsentRequestStatusType.Accepted)
                {
                    await SetExternalIdentities(details, cancellationToken);
                    return details;
                }

                if (details.ConsentRequestStatus != ConsentRequestStatusType.Created)
                {
                    return Problems.ConsentCantBeAccepted;
                }

                throw;
            }

            ConsentRequestDetails updated = await _consentRepository.GetRequest(consentRequestId, cancellationToken);
            await SetExternalIdentities(updated, cancellationToken);
            return updated;
        }

        /// <inheritdoc/>
        public async Task<Result<ConsentRequestDetails>> RevokeConsent(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken)
        {
            ConsentRequestDetails details = await _consentRepository.GetRequest(consentRequestId, cancellationToken);
            if (details.ConsentRequestStatus == ConsentRequestStatusType.Revoked)
            {
                await SetExternalIdentities(details, cancellationToken);
                return details;
            }

            if (details.ConsentRequestStatus != ConsentRequestStatusType.Accepted)
            {
                return Problems.ConsentCantBeRevoked;
            }

            try
            {
                await _consentRepository.Revoke(consentRequestId, performedByParty, cancellationToken);
            }
            catch (Exception)
            {
                details = await _consentRepository.GetRequest(consentRequestId, cancellationToken);

                if (details.ConsentRequestStatus == ConsentRequestStatusType.Revoked)
                {
                    await SetExternalIdentities(details, cancellationToken);
                    return details;
                }

                if (details.ConsentRequestStatus != ConsentRequestStatusType.Accepted)
                {
                    return Problems.ConsentCantBeRevoked;
                }

                throw;
            }

            ConsentRequestDetails updated = await _consentRepository.GetRequest(consentRequestId, cancellationToken);
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
            else if (consentPartyUrn.IsOrganizationId(out Authorization.Core.Models.Register.OrganizationNumber organizationNumber))
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
                return ConsentPartyUrn.OrganizationId.Create(Authorization.Core.Models.Register.OrganizationNumber.Parse(party.OrganizationId));
            }

            throw new ArgumentException($"Party with guid {guid} is not valid consent party");
        }

        private async Task<ConsentPartyUrn> GetInternalIdentifier(Authorization.Core.Models.Register.OrganizationNumber organizationNumber, CancellationToken cancellationToken)
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
        /// </summary>
        private async Task<bool> AuthorizeUserForConsentRequest(Guid userUuid, ConsentRequestDetails consentRequest, CancellationToken cancellationToken)
        {
            Guid fromParty = consentRequest.From.IsPartyUuid(out Guid from) ? from : Guid.Empty;
            List<Party> parties = await _partiesClient.GetPartiesAsync(new List<Guid> { fromParty }, cancellationToken: cancellationToken);
            Party party = parties[0];

            UserProfile profile = await _profileClient.GetUser(new Models.Profile.UserProfileLookup() { UserUuid = userUuid }, cancellationToken);
            if (profile == null)
            {
                return false;
            }

            foreach (ConsentRight consentRight in consentRequest.ConsentRights)
            {
                if (!await AuthorizeForConsentRight(party, profile, consentRight))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> AuthorizeForConsentRight(Party party, UserProfile profile, ConsentRight consentRight)
        {
            DelegationCheckResponse response = await GetDelegatableRightsForConsentResource(party, profile, consentRight);

            if (response.RightDelegationCheckResults != null)
            {
                foreach (string action in consentRight.Action)
                {
                    bool actionMatch = false;
                    foreach (RightDelegationCheckResult result in response.RightDelegationCheckResults)
                    {
                        if (result.Action.Value.Equals(action, StringComparison.InvariantCultureIgnoreCase) && result.Status.Equals(DelegableStatus.Delegable))
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
            }
            else
            {
                return false;
            }

            return true;
        }

        private async Task<DelegationCheckResponse> GetDelegatableRightsForConsentResource(Party party, UserProfile profile, ConsentRight consentRight)
        {
            RightsDelegationCheckRequest rightsDelegationCheckRequest = new()
            {
                From = [new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, Value = party.PartyId.ToString() }]
            };

            if (consentRight.Resource != null && consentRight.Resource.Count == 1)
            {
                // A ConsentRight Should only have one resource.  Currently no support for subresources as part of a consent request.
                ConsentResourceAttribute resource = consentRight.Resource[0];
                rightsDelegationCheckRequest.Resource = [new AttributeMatch { Id = resource.Type, Value = resource.Value }];
            }
            else
            {
                return null;
            }

            return await _singleRightsService.RightsDelegationCheck(profile.UserId, 3, rightsDelegationCheckRequest);
        }

        /// <summary>
        /// Validates and sets internal identifiers for the consent request
        /// - Validates that the from and to party is valid
        /// - Validates that resources requested in consent is valid
        /// - Validates that valid to time is valid
        /// </summary>
        private async Task<Result<ConsentRequest>> ValidateAndSetInternalIdentifiers(ConsentRequest consentRequest, CancellationToken cancelactionToken)
        {
            ValidationErrorBuilder validationErrorsBuilder = default;
            MultipleProblemBuilder problemsBuilder = default;
            Result<ConsentPartyUrn> fromParty = await ValidatePartyFromExternalIdentity(consentRequest.From, cancelactionToken);
            if (fromParty.IsProblem)
            {
                return fromParty.Problem;
            }
            else
            {
                consentRequest.From = fromParty.Value;
            }

            Result<ConsentPartyUrn> toParty = await ValidatePartyFromExternalIdentity(consentRequest.To, cancelactionToken);
            if (toParty.IsProblem)
            {
                return toParty.Problem;
            }
            else
            {
                consentRequest.To = toParty.Value;
            }

            validationErrorsBuilder = ValidateValidTo(consentRequest, validationErrorsBuilder);

            string templateId = string.Empty;

            if (consentRequest.ConsentRights == null || consentRequest.ConsentRights.Count == 0)
            {
                validationErrorsBuilder.Add(ValidationErrors.MissingConsentRight, ResourceParam);
            }
            else
            {
                templateId = string.Empty;
                for (int rightIndex = 0; rightIndex < consentRequest.ConsentRights.Count; rightIndex++)
                {
                    (problemsBuilder, templateId) = await ValidateConsentRight(consentRequest, problemsBuilder, rightIndex, templateId, cancelactionToken);
                }
            }

            if (validationErrorsBuilder.TryBuild(out var errorResult))
            {
                problemsBuilder.Add(errorResult);
            }

            if (problemsBuilder.TryBuild(out var problemResult))
            {
                return problemResult;
            }

            ConsentTemplate consentTemplate = await GetTemplate(templateId, cancelactionToken);

            consentRequest.TemplateId = consentTemplate.Version;

            return consentRequest;
        }

        private void ValidateContext(ConsentRequestDetails consentRequest, ConsentContext context, ValidationErrorBuilder errors)
        {
            if (context == null)
            {
                errors.Add(ValidationErrors.Required, "Context");
            }
       
            if (consentRequest.ConsentRights.Count != context.ConsentContextResources.Count)
            {
                errors.Add(ValidationErrors.InvalidResourceContext, "Context");
            }

            foreach (ConsentRight consentRight in consentRequest.ConsentRights)
            {
                ConsentResourceAttribute attribute = consentRight.Resource[0];
                string resourceId = $"{attribute.Type}:{attribute.Value}";
                if (context.ConsentContextResources.All(x => x.ResourceId != resourceId))
                {
                    errors.Add(ValidationErrors.InvalidResourceContext, ResourceParam);
                }
            }
        }

        private async Task<(MultipleProblemBuilder Errors, string TemplateId)> ValidateConsentRight(ConsentRequest consentRequest, MultipleProblemBuilder problemsBuilder, int rightIndex, string templateId, CancellationToken cancelactionToken)
        {
            ConsentRight consentRight = consentRequest.ConsentRights[rightIndex];
            ValidationErrorBuilder validationErrors = default;

            if (consentRight.Action == null || consentRight.Action.Count == 0)
            {
                validationErrors.Add(ValidationErrors.MissingAction, $"/consentRight/{rightIndex}/action");
            }

            if (consentRight.Resource == null || consentRight.Resource.Count == 0 || consentRight.Resource.Count > 1)
            {
                problemsBuilder.Add(Problems.InvalidConsentResource);
            }
            else
            {
                ServiceResource resourceDetails = await _resourceRegistryClient.GetResource(consentRight.Resource[0].Value, cancelactionToken);
                if (resourceDetails == null)
                {
                    problemsBuilder.Add(Problems.InvalidConsentResource);
                }
                else if (!resourceDetails.ResourceType.Equals(ResourceType.Consentresource))
                {
                    problemsBuilder.Add(Problems.InvalidConsentResource);
                }
                else
                {
                    ValidateConsentMetadata(ref problemsBuilder, rightIndex, consentRight, resourceDetails);
                }

                if (string.IsNullOrEmpty(templateId))
                {
                    templateId = resourceDetails.ConsentTemplate;
                }
                else if (!templateId.Equals(resourceDetails.ConsentTemplate, StringComparison.InvariantCultureIgnoreCase))
                {
                    problemsBuilder.Add(Problems.InvalidResourceCombination);
                }
            }

            if (validationErrors.TryBuild(out var errorResult))
            {
                problemsBuilder.Add(errorResult);
            }

            return (problemsBuilder, templateId);
        }

        private static void ValidateConsentMetadata(ref MultipleProblemBuilder problemsBuilder, int rightIndex, ConsentRight consentRight, ServiceResource resourceDetails)
        {
            if (consentRight.MetaData != null && consentRight.MetaData.Count > 0)
            {
                foreach (KeyValuePair<string, string> metaData in consentRight.MetaData)
                {
                    if (resourceDetails.ConsentMetadata == null || !resourceDetails.ConsentMetadata.ContainsKey(metaData.Key.ToLower()))
                    {
                        problemsBuilder.Add(Problems.UnknownConsentMetadata.Create([new("key", metaData.Key.ToLower())]));
                    }

                    if (string.IsNullOrEmpty(metaData.Value))
                    {
                        problemsBuilder.Add(Problems.MissingMetadataValue.Create([new("rightindex", rightIndex.ToString())]));
                    }
                }
            }

            ValidateRequiredMetadata(ref problemsBuilder, rightIndex, consentRight, resourceDetails);
        }

        private static void ValidateRequiredMetadata(ref MultipleProblemBuilder problemsBuilder, int rightIndex, ConsentRight consentRight, ServiceResource resourceDetails)
        {
            if (resourceDetails.ConsentMetadata != null)
            {
                foreach (string key in resourceDetails.ConsentMetadata.Keys.Select(consentMetadata => consentMetadata))
                {
                    if (consentRight.MetaData == null || !consentRight.MetaData.ContainsKey(key))
                    {
                        problemsBuilder.Add(Problems.MissingMetadata.Create([new("key", key.ToLower())]));
                    }
                }
            }

        }

        private static ValidationErrorBuilder ValidateValidTo(ConsentRequest consentRequest, ValidationErrorBuilder errors)
        {
            if (consentRequest.ValidTo < DateTime.UtcNow)
            {
                errors.Add(ValidationErrors.TimeNotInFuture, "ValidTo");
            }

            return errors;
        }

        private async Task<Result<ConsentPartyUrn>> ValidatePartyFromExternalIdentity(ConsentPartyUrn consentPartyUrn, CancellationToken cancelactionToken)
        {
            ConsentPartyUrn to = await MapFromExternalIdenity(consentPartyUrn, cancelactionToken);
            if (to == null)
            {
                if (consentPartyUrn.IsOrganizationId(out _))
                {
                    return Problems.InvalidOrganizationIdentifier.Create([new("orgnr", consentPartyUrn.ToString())]);
                }
                else if (consentPartyUrn.IsPersonId(out _))
                {
                    return Problems.InvalidPersonIdentifier.Create([new("fnumber", consentPartyUrn.ToString())]);
                }
            }
            
            return to;
        }

        private async Task<ConsentTemplate> GetTemplate(string templateId, CancellationToken cancellationToken)
        {
            return new ConsentTemplate()
            {
                Version = Guid.CreateVersion7(),
                Id = templateId,
                Texts = new ConsentTemplateTexts() // Ensure the required 'Texts' property is initialized  
                {
                    // Initialize the properties of ConsentTemplateTexts as needed  
                }
            };
        }
    }
}
