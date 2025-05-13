using System.Security.Claims;
using System.Text.Json;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Register;

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

            try
            {
                using JsonDocument doc = JsonDocument.Parse(consumerJson);
                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("ID", out JsonElement idElement) ||
                    !root.TryGetProperty("authority", out JsonElement authorityElement))
                {
                    return null;
                }

                string consumerAuthority = authorityElement.GetString()!;
                if (!"iso6523-actorid-upis".Equals(consumerAuthority))
                {
                    return null;
                }

                string consumerId = idElement.GetString()!;
                string organisationNumber = consumerId.Split(':')[1];
                return ConsentPartyUrn.OrganizationId.Create(OrganizationNumber.Parse(organisationNumber));
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
