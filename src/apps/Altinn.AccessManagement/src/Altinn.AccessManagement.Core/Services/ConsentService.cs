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
            return await _consentRepository.CreateRequest(consentRequest);
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
    }
}
