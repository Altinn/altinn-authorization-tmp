using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Register;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace Altinn.AccessManagement.Api.Enterprise.Utils
{
    /// <summary>
    /// Utility class for user related operations
    /// </summary>
    public static class OrgUtil
    {
        /// <summary>
        /// Returns partyUid claim value if present. Null if not
        /// </summary>
        public static ConsentPartyUrn? GetAuthenticatedParty(ClaimsPrincipal claimsPrincipal)
        {
            string? consumerJson = claimsPrincipal.FindFirstValue("consumer");
            if (string.IsNullOrEmpty(consumerJson))
            {
                return null;
            }

            JObject consumer = JObject.Parse(consumerJson);

            string consumerAuthority = consumer["authority"].ToString();
            if (!"iso6523-actorid-upis".Equals(consumerAuthority))
            {
                return null;
            }

            string consumerId = consumer["ID"].ToString();

            string organisationNumber = consumerId.Split(":")[1];
            return ConsentPartyUrn.OrganizationId.Create(OrganizationNumber.Parse(organisationNumber));
        }
    }
}
