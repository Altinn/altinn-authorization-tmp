using System.Security.Claims;
using System.Text.Json;
using Altinn.AccessManagement.Api.Enduser.Authorization.AuthorizationRequirement;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Common.PEP.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using static Altinn.AccessManagement.Core.Constants.AltinnXacmlConstants;

namespace Altinn.AccessManagement.Api.Enduser.Authorization.Helper
{
    /// <summary>
    /// Helper class for decision
    /// </summary>
    public class DecisionHelper
    {
        private const string ParamParty = "party";
        private const string ParamFrom = "from";
        private const string ParamTo = "to";
        private const string DefaultIssuer = "Altinn";
        private const string DefaultType = "string";

        private const string PolicyObligationMinAuthnLevel = "urn:altinn:minimum-authenticationlevel";
        private const string PolicyObligationMinAuthnLevelOrg = "urn:altinn:minimum-authenticationlevel-org";

        private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Creates a decision request based on input
        /// </summary>
        /// <returns></returns>
        public static XacmlJsonRequestRoot CreateDecisionRequest(AuthorizationHandlerContext context, EndUserResourceAccessRequirement requirement, IQueryCollection queryParams)
        {
            XacmlJsonRequest request = new XacmlJsonRequest();
            request.AccessSubject = new List<XacmlJsonCategory>();
            request.Action = new List<XacmlJsonCategory>();
            request.Resource = new List<XacmlJsonCategory>();

            string? party = queryParams.FirstOrDefault(p => p.Key == ParamParty).Value.FirstOrDefault();

            request.AccessSubject.Add(CreateSubjectCategory(context.User.Claims));
            request.Action.Add(CreateActionCategory(requirement.ActionType));

            Guid? partyUuid = TryParseUuid(party);
            if (partyUuid.HasValue)
            {
                request.Resource.Add(CreateResourceCategoryForResource(partyUuid));
            }
            else
            {
                throw new ArgumentException("invalid party " + party);
            }

            XacmlJsonRequestRoot jsonRequest = new XacmlJsonRequestRoot() { Request = request };

            return jsonRequest;
        }

        /// <summary>
        /// Validate the response from PDP
        /// </summary>
        /// <param name="results">The response to validate</param>
        /// <param name="user">The <see cref="ClaimsPrincipal"/></param>
        /// <returns>true or false, valid or not</returns>
        public static bool ValidatePdpDecision(List<XacmlJsonResult> results, ClaimsPrincipal user)
        {
            if (results == null)
            {
                throw new ArgumentNullException("results");
            }

            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            // We request one thing and then only want one result
            if (results.Count != 1)
            {
                return false;
            }

            return ValidateDecisionResult(results.First(), user);
        }

        /// <summary>
        /// Gets the users partyUuid
        /// </summary>
        /// <param name="context">the http context</param>
        /// <returns>the logged in users id</returns>
        public static Guid GetUserPartyUuid(HttpContext context)
        {
            var claim = context.User?.Claims.FirstOrDefault(c => c.Type.Equals(AltinnCoreClaimTypes.PartyUuid));
            if (claim != null && Guid.TryParse(claim.Value, out Guid partyUuid))
            {
                return partyUuid;
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Gets the users id
        /// </summary>
        /// <param name="context">the http context</param>
        /// <returns>the logged in users id</returns>
        public static int GetUserId(HttpContext context)
        {
            var claim = context.User?.Claims.FirstOrDefault(c => c.Type.Equals(AltinnCoreClaimTypes.UserId));
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }

            return 0;
        }

        /// <summary>
        /// Gets the 'from' parameter from the query string
        /// </summary>
        /// <param name="context">The HTTP context containing the request</param>
        /// <returns>The 'from' parameter as a Guid if valid, otherwise null</returns>
        public static Guid? GetFromParam(HttpContext context)
        {
            string? from = context.Request.Query[ParamFrom];
            if (Guid.TryParse(from, out Guid fromGuid))
            {
                return fromGuid;
            }

            return null;
        }

        /// <summary>
        /// Gets the 'to' parameter from the query string
        /// </summary>
        /// <param name="context">The HTTP context containing the request</param>
        /// <returns>The 'to' parameter as a Guid if valid, otherwise null</returns>
        public static Guid? GetToParam(HttpContext context)
        {
            string? from = context.Request.Query[ParamTo];
            if (Guid.TryParse(from, out Guid toGuid))
            {
                return toGuid;
            }

            return null;
        }

        /// <summary>
        /// Gets the 'party' parameter from the query string
        /// </summary>
        /// <param name="context">The HTTP context containing the request</param>
        /// <returns>The 'party' parameter as a Guid if valid, otherwise null</returns>
        public static Guid? GetPartyParam(HttpContext context)
        {
            string? from = context.Request.Query[ParamParty];
            if (Guid.TryParse(from, out Guid partyGuid))
            {
                return partyGuid;
            }

            return null;
        }

        /// <summary>
        /// Validate the response from PDP
        /// </summary>
        /// <param name="result">The response to validate</param>
        /// <param name="user">The <see cref="ClaimsPrincipal"/></param>
        /// <returns>true or false, valid or not</returns>
        private static bool ValidateDecisionResult(XacmlJsonResult result, ClaimsPrincipal user)
        {
            // Checks that the result is nothing else than "permit"
            if (!result.Decision.Equals(XacmlContextDecision.Permit.ToString()))
            {
                return false;
            }

            // Checks if the result contains obligation
            if (result.Obligations != null)
            {
                List<XacmlJsonObligationOrAdvice> obligationList = result.Obligations;
                XacmlJsonAttributeAssignment attributeMinLvAuth = GetObligation(PolicyObligationMinAuthnLevel, obligationList);

                // Checks if the obligation contains a minimum authentication level attribute
                if (attributeMinLvAuth != null)
                {
                    string minAuthenticationLevel = attributeMinLvAuth.Value;
                    string usersAuthenticationLevel = user.Claims.FirstOrDefault(c => c.Type.Equals("urn:altinn:authlevel")).Value;

                    // Checks that the user meets the minimum authentication level
                    if (Convert.ToInt32(usersAuthenticationLevel) < Convert.ToInt32(minAuthenticationLevel))
                    {
                        if (user.Claims.FirstOrDefault(c => c.Type.Equals("urn:altinn:org")) != null)
                        {
                            XacmlJsonAttributeAssignment attributeMinLvAuthOrg = GetObligation(PolicyObligationMinAuthnLevelOrg, obligationList);
                            if (attributeMinLvAuthOrg != null)
                            {
                                if (Convert.ToInt32(usersAuthenticationLevel) >= Convert.ToInt32(attributeMinLvAuthOrg.Value))
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        private static XacmlJsonAttributeAssignment? GetObligation(string category, List<XacmlJsonObligationOrAdvice> obligations)
        {
            foreach (XacmlJsonObligationOrAdvice obligation in obligations)
            {
                XacmlJsonAttributeAssignment? assignment = obligation.AttributeAssignment.FirstOrDefault(a => a.Category.Equals(category));
                if (assignment != null)
                {
                    return assignment;
                }
            }

            return null;
        }

        private static List<XacmlJsonAttribute> CreateSubjectAttributes(IEnumerable<Claim> claims)
        {
            List<XacmlJsonAttribute> attributes = new List<XacmlJsonAttribute>();

            XacmlJsonAttribute userIdAttribute = null;
            XacmlJsonAttribute personUuidAttribute = null;
            XacmlJsonAttribute partyIdAttribute = null;
            XacmlJsonAttribute resourceIdAttribute = null;
            XacmlJsonAttribute legacyOrganizationNumberAttibute = null;
            XacmlJsonAttribute organizationNumberAttribute = null;
            XacmlJsonAttribute systemUserAttribute = null;

            // Mapping all claims on user to attributes
            foreach (Claim claim in claims)
            {
                if (IsCamelCaseOrgnumberClaim(claim.Type))
                {
                    // Set by Altinn authentication this format
                    legacyOrganizationNumberAttibute = CreateXacmlJsonAttribute(MatchAttributeIdentifiers.OrganizationNumberAttribute, claim.Value, DefaultType, claim.Issuer);
                    organizationNumberAttribute = CreateXacmlJsonAttribute(MatchAttributeIdentifiers.OrganizationId, claim.Value, DefaultType, claim.Issuer);
                }
                else if (IsScopeClaim(claim.Type))
                {
                    attributes.Add(CreateXacmlJsonAttribute(MatchAttributeIdentifiers.Scope, claim.Value, DefaultType, claim.Issuer));
                }
                else if (IsJtiClaim(claim.Type))
                {
                    attributes.Add(CreateXacmlJsonAttribute(MatchAttributeIdentifiers.SessionId, claim.Value, DefaultType, claim.Issuer));
                }
                else if (IsSystemUserClaim(claim, out SystemUserClaim? userClaim))
                {
                    systemUserAttribute = CreateXacmlJsonAttribute(MatchAttributeIdentifiers.SystemUserUuid, userClaim.Systemuser_id[0], DefaultType, claim.Issuer);
                }
                else if (IsUserIdClaim(claim.Type))
                {
                    userIdAttribute = CreateXacmlJsonAttribute(MatchAttributeIdentifiers.UserAttribute, claim.Value, DefaultType, claim.Issuer);
                }
                else if (IsPersonUuidClaim(claim.Type))
                {
                    personUuidAttribute = CreateXacmlJsonAttribute(MatchAttributeIdentifiers.PersonUuid, claim.Value, DefaultType, claim.Issuer);
                }
                else if (IsPartyIdClaim(claim.Type))
                {
                    partyIdAttribute = CreateXacmlJsonAttribute(MatchAttributeIdentifiers.PartyAttribute, claim.Value, DefaultType, claim.Issuer);
                }
                else if (IsResourceClaim(claim.Type))
                {
                    partyIdAttribute = CreateXacmlJsonAttribute(MatchAttributeIdentifiers.ResourceRegistryAttribute, claim.Value, DefaultType, claim.Issuer);
                }
                else if (IsOrganizationNumberAttributeClaim(claim.Type))
                {
                    // If claimlist contains new format of orgnumber reset any old. To ensure there is not a mismatch
                    organizationNumberAttribute = CreateXacmlJsonAttribute(MatchAttributeIdentifiers.OrganizationId, claim.Value, DefaultType, claim.Issuer);
                    legacyOrganizationNumberAttibute = null;
                }
                else if (IsValidUrn(claim.Type))
                {
                    attributes.Add(CreateXacmlJsonAttribute(claim.Type, claim.Value, DefaultType, claim.Issuer));
                }
            }

            // Adding only one of the subject attributes to make sure we dont have mismatching duplicates for PDP request that potentially could cause issues
            if (personUuidAttribute != null)
            {
                attributes.Add(personUuidAttribute);
            }
            else if (userIdAttribute != null)
            {
                attributes.Add(userIdAttribute);
            }
            else if (partyIdAttribute != null)
            {
                attributes.Add(partyIdAttribute);
            }
            else if (resourceIdAttribute != null)
            {
                attributes.Add(resourceIdAttribute);
            }
            else if (systemUserAttribute != null)
            {
                // If we have a system user we only add that. No other attributes allowed by PDP
                attributes.Clear();
                attributes.Add(systemUserAttribute);
            }
            else if (legacyOrganizationNumberAttibute != null)
            {
                // For legeacy we set both
                attributes.Add(legacyOrganizationNumberAttibute);
                attributes.Add(organizationNumberAttribute);
            }
            else if (organizationNumberAttribute != null)
            {
                attributes.Add(organizationNumberAttribute);
            }

            return attributes;
        }

        private static XacmlJsonCategory CreateResourceCategoryForResource(Guid? partyUuid, bool includeResult = false)
        {
            XacmlJsonCategory resourceCategory = new XacmlJsonCategory();
            resourceCategory.Attribute = new List<XacmlJsonAttribute>();

            if (partyUuid.HasValue)
            {
                resourceCategory.Attribute.Add(CreateXacmlJsonAttribute(MatchAttributeIdentifiers.PartyUuidAttribute, partyUuid.Value.ToString(), DefaultType, DefaultIssuer, includeResult));
            }

            return resourceCategory;
        }

        private static XacmlJsonAttribute CreateXacmlJsonAttribute(string attributeId, string value, string dataType, string issuer, bool includeResult = false)
        {
            XacmlJsonAttribute xacmlJsonAttribute = new XacmlJsonAttribute();

            xacmlJsonAttribute.AttributeId = attributeId;
            xacmlJsonAttribute.Value = value;
            xacmlJsonAttribute.DataType = dataType;
            xacmlJsonAttribute.Issuer = issuer;
            xacmlJsonAttribute.IncludeInResult = includeResult;

            return xacmlJsonAttribute;
        }

        private static Guid? TryParseUuid(string party)
        {
            Guid partyUuid;
            if (!Guid.TryParse(party, out partyUuid))
            {
                return null;
            }

            return partyUuid;
        }

        /// <summary>
        /// Create a new <see cref="XacmlJsonCategory"/> with a list of subject attributes based on the given claims.
        /// </summary>
        /// <param name="claims">The list of claims</param>
        /// <returns>A populated subject category</returns>
        private static XacmlJsonCategory CreateSubjectCategory(IEnumerable<Claim> claims)
        {
            XacmlJsonCategory subjectAttributes = new XacmlJsonCategory();
            subjectAttributes.Attribute = CreateSubjectAttributes(claims);

            return subjectAttributes;
        }

        /// <summary>
        /// Create a new <see cref="XacmlJsonCategory"/> attribute of type Action with the given action type
        /// </summary>
        /// <param name="actionType">The action type</param>
        /// <param name="includeResult">A value indicating whether the value should be included in the result.</param>
        /// <returns>The created category</returns>
        private static XacmlJsonCategory CreateActionCategory(string actionType, bool includeResult = false)
        {
            XacmlJsonCategory actionAttributes = new XacmlJsonCategory();
            actionAttributes.Attribute = new List<XacmlJsonAttribute>();
            actionAttributes.Attribute.Add(CreateXacmlJsonAttribute(MatchAttributeIdentifiers.ActionId, actionType, DefaultType, DefaultIssuer, includeResult));
            return actionAttributes;
        }

        private static bool IsCamelCaseOrgnumberClaim(string name)
        {
            return name.Equals("urn:altinn:orgNumber");
        }

        private static bool IsScopeClaim(string name)
        {
            return name.Equals("scope");
        }

        private static bool IsJtiClaim(string name)
        {
            return name.Equals("jti");
        }

        private static bool IsSystemUserClaim(Claim claim, out SystemUserClaim? userClaim)
        {
            if (claim.Type.Equals("authorization_details"))
            {
                userClaim = JsonSerializer.Deserialize<SystemUserClaim>(claim.Value, Options);
                if (userClaim?.Systemuser_id != null && userClaim.Systemuser_id.Count > 0)
                {
                    return true;
                }

                return false;
            }
            else
            {
                userClaim = null;
                return false;
            }
        }

        private static bool IsUserIdClaim(string name)
        {
            return name.Equals(MatchAttributeIdentifiers.UserAttribute);
        }

        private static bool IsPersonUuidClaim(string name)
        {
            return name.Equals(MatchAttributeIdentifiers.PersonUuid);
        }

        private static bool IsPartyIdClaim(string name)
        {
            return name.Equals(MatchAttributeIdentifiers.PartyAttribute);
        }

        private static bool IsResourceClaim(string name)
        {
            return name.Equals(MatchAttributeIdentifiers.ResourceRegistryAttribute);
        }

        private static bool IsOrganizationNumberAttributeClaim(string name)
        {
            // The new format of orgnumber
            return name.Equals(MatchAttributeIdentifiers.OrganizationId);
        }

        private static bool IsValidUrn(string value)
        {
            return value.StartsWith("urn:", StringComparison.Ordinal);
        }
    }
}
