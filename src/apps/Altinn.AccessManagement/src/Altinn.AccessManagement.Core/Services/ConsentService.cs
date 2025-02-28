using Altinn.AccessManagement.Core.Enums.Consent;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.ProblemDetails;
using Altinn.Register.Core.Parties;

namespace Altinn.AccessManagement.Core.Services
{
    /// <summary>
    /// Service for handling consent
    /// </summary>
    public class ConsentService : IConsent
    {
        private readonly IConsentRepository _consentRepository;

        /// <summary>
        /// 
        /// </summary>
        public ConsentService(IConsentRepository consentRepository)
        {
            _consentRepository = consentRepository;
        }

        /// <inheritdoc/>
        public Task ApproveRequest(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<Result<ConsentRequestDetails>> CreateRequest(ConsentRequest consentRequest)
        {
            consentRequest.From = MapFromExternalIdenity(consentRequest.From);
            consentRequest.To = MapFromExternalIdenity(consentRequest.To);
            ConsentRequestDetails requestDetails = await _consentRepository.CreateRequest(consentRequest);
            requestDetails.From = MapToExternalIdenity(requestDetails.From);
            requestDetails.To = MapToExternalIdenity(requestDetails.To);
            return requestDetails;
        }

        /// <inheritdoc/>
        public Task DeleteRequest(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<Consent> GetConcent(Guid id, string from, string to)
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

            return Task.FromResult(consent);
        }



        private ConsentPartyUrn MapFromExternalIdenity(ConsentPartyUrn consentPartyUrn)
        {
            if (consentPartyUrn.IsPersonId(out PersonIdentifier personIdentifier))
            {
                return GetInternalIdentifier(personIdentifier);
            }
            else if (consentPartyUrn.IsOrganizationId(out OrganizationNumber organizationNumber))
            {
                return GetInternalIdentifier(organizationNumber);
            }
            return consentPartyUrn;
        }

        private ConsentPartyUrn MapToExternalIdenity(ConsentPartyUrn consentPartyUrn)
        {
            if (consentPartyUrn.IsPartyUuid(out Guid partyUuid))
            {
                return GetExternalIdentifier(partyUuid);    
            }

            return consentPartyUrn;
        }

        private ConsentPartyUrn GetExternalIdentifier(Guid guid)
        {
            return ConsentPartyUrn.PersonId.Create(PersonIdentifier.Parse("01014922047"));
        }

        private ConsentPartyUrn GetInternalIdentifier(OrganizationNumber organizationNumber)
        {
            return ConsentPartyUrn.PartyUuid.Create(Guid.NewGuid());
        }

        private ConsentPartyUrn GetInternalIdentifier(PersonIdentifier personIdentifier)
        {
            return ConsentPartyUrn.PartyUuid.Create(Guid.NewGuid());
        }
    }
}
