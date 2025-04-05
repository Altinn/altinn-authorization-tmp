using System.Security.Claims;

namespace Altinn.AccessManagement.Api.Enduser.Utils
{
    /// <summary>
    /// Utility class for user related operations
    /// </summary>
    public static class UserUtil
    {
        /// <summary>
        /// Returns partyUid claim value if present. Null if not
        /// </summary>
        public static Guid? GetUserUuid(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal != null && claimsPrincipal.Claims != null)
            {
                foreach (Claim claim in claimsPrincipal.Claims)
                {
                    if (claim.Type.Equals("urn:altinn:party:uuid"))
                    {
                        if (Guid.TryParse(claim.Value, out Guid partyUid))
                        {
                            return partyUid;
                        }
                    }
                }
            }

            return null;
        }    
    }
}
