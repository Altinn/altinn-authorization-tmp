using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Register;
using Altinn.Authorization.ProblemDetails;
using Altinn.Platform.Register.Enums;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Services
{
    /// <summary>
    /// Service for handling consent
    /// </summary>
    /// <remarks>
    /// Service responsible for consent functionality
    /// </remarks>
    public class ConsentService(IConsentRepository consentRepository, IPartiesClient partiesClient) : IConsent
    {
        private readonly IConsentRepository _consentRepository = consentRepository;
        private readonly IPartiesClient _partiesClient = partiesClient;

        /// <inheritdoc/>
        public async Task<Result<ConsentRequestDetails>> CreateRequest(ConsentRequest consentRequest, CancellationToken cancellationToken = default)
        {
            consentRequest.From = await MapFromExternalIdenity(consentRequest.From);
            consentRequest.To = await MapFromExternalIdenity(consentRequest.To);
            ConsentRequestDetails requestDetails = await _consentRepository.CreateRequest(consentRequest);
            requestDetails.From = consentRequest.From;
            requestDetails.To = consentRequest.To;
            return requestDetails;
        }

        /// <inheritdoc/>
        public Task DenyRequest(Guid id, Guid performedBy, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<Consent> GetConcent(Guid id, ConsentPartyUrn from, ConsentPartyUrn to, CancellationToken cancellationToken = default)
        {
            Consent consent = new Consent
            {
                Id = id,
                From = ConsentPartyUrn.PersonId.Create(PersonIdentifier.Parse("01014922047")),
                To = ConsentPartyUrn.OrganizationId.Create(OrganizationNumber.Parse("910194143")),
                ConcentRights = new List<ConsentRight>
                {
                    new ConsentRight()
                    {
                        Action = ["read"],
                        Resource = new List<ConsentResourceAttribute>
                        {
                            new ConsentResourceAttribute
                            {
                                Type = "urn:altinn:resource",
                                Value = "skd_inntektsnfo"
                            }
                        },
                        MetaData = new Dictionary<string, string>
                        {
                            { "inntektsaar", "2024" }
                        }
                    }
                }
            };

            consent.From = await MapToExternalIdenity(consent.From);
            consent.To = await MapToExternalIdenity(consent.To);

            return consent;
        }

        /// <inheritdoc/>
        public async Task<ConsentRequestDetails> GetRequest(Guid id, CancellationToken cancellationToken = default)
        {
            ConsentRequestDetails details = await _consentRepository.GetRequest(id, cancellationToken);
            details.To = await MapToExternalIdenity(details.To, cancellationToken);
            details.From = await MapToExternalIdenity(details.From, cancellationToken);
            return details;
        }

        /// <inheritdoc/>
        public async Task ApproveRequest(Guid id, Guid approvedByParty, CancellationToken cancellationToken = default)
        {
            await _consentRepository.ApproveConsentRequest(id, cancellationToken);
        }

        /// <inheritdoc/>
        public Task RevokeConsent(Guid id, Guid performedByParty, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private async Task<ConsentPartyUrn> MapFromExternalIdenity(ConsentPartyUrn consentPartyUrn)
        {
            if (consentPartyUrn.IsPersonId(out PersonIdentifier personIdentifier))
            {
                return await GetInternalIdentifier(personIdentifier);
            }
            else if (consentPartyUrn.IsOrganizationId(out OrganizationNumber organizationNumber))
            {
                return await GetInternalIdentifier(organizationNumber);
            }

            return consentPartyUrn;
        }

        private async Task<ConsentPartyUrn> MapToExternalIdenity(ConsentPartyUrn consentPartyUrn, CancellationToken cancellationToken = default)
        {
            if (consentPartyUrn.IsPartyUuid(out Guid partyUuid))
            {
                return await GetExternalIdentifier(partyUuid, cancellationToken);    
            }

            return consentPartyUrn;
        }

        private async Task<ConsentPartyUrn> GetExternalIdentifier(Guid guid, CancellationToken cancellationToken = default)
        {
            List<Party> parties = await _partiesClient.GetPartiesAsync(new List<Guid> { guid });

            if (parties.Count == 0)
            {
                throw new ArgumentException($"Party with guid {guid} not found");
            }

            Party party = parties.First();

            if (party.PartyTypeName.Equals(PartyType.Organisation))
            {
                return ConsentPartyUrn.OrganizationId.Create(OrganizationNumber.Parse(party.OrgNumber));
            }
            else if (party.PartyTypeName.Equals(PartyType.Person))
            {
                return ConsentPartyUrn.PersonId.Create(PersonIdentifier.Parse(party.SSN));
            }

            throw new ArgumentException($"Party with guid {guid} is not valid consent party");
        }

        private async Task<ConsentPartyUrn> GetInternalIdentifier(OrganizationNumber organizationNumber, CancellationToken cancellationToken = default)
        {
            Party party = await _partiesClient.LookupPartyBySSNOrOrgNo(new PartyLookup { OrgNo = organizationNumber.ToString() }, cancellationToken);
            if (party == null || party.PartyUuid == null)
            {
                throw new ArgumentException($"Party with orgNo {organizationNumber} not found");
            }

            return ConsentPartyUrn.PartyUuid.Create(party.PartyUuid.Value);
        }

        private async Task<ConsentPartyUrn> GetInternalIdentifier(PersonIdentifier personIdentifier, CancellationToken cancellationToken = default)
        {
            Party party = await _partiesClient.LookupPartyBySSNOrOrgNo(new PartyLookup { Ssn = personIdentifier.ToString() }, cancellationToken);
            if (party == null || party.PartyUuid == null)
            {
                throw new ArgumentException($"Party with ssn {personIdentifier} not found");
            }

            return ConsentPartyUrn.PartyUuid.Create(party.PartyUuid.Value);
        }
    }
}
