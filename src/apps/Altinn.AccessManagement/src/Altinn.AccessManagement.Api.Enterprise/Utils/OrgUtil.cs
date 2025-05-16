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
            if (string.IsNullOrWhiteSpace(consumerJson))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(consumerJson);
                var root = doc.RootElement;

                if (!root.TryGetProperty("ID", out var idElement) ||
                    !root.TryGetProperty("authority", out var authorityElement))
                {
                    return null;
                }

                var consumerAuthority = authorityElement.GetString();
                if (!string.Equals(consumerAuthority, "iso6523-actorid-upis", StringComparison.Ordinal))
                {
                    return null;
                }

                var consumerId = idElement.GetString();
                if (string.IsNullOrEmpty(consumerId) || !consumerId.Contains(':'))
                {
                    return null;
                }

                var parts = consumerId.Split(':');
                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[1]))
                {
                    return null;
                }

                var organisationNumber = parts[1];
                return ConsentPartyUrn.OrganizationId.Create(OrganizationNumber.Parse(organisationNumber));
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
