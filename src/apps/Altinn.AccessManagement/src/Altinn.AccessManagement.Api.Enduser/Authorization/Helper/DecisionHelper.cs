﻿using System.Security.Claims;
using System.Text.Json;
using Altinn.AccessManagement.Api.Enduser.Authorization.AuthorizationRequirement;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Microsoft.AspNetCore.Authorization;
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

            string party = queryParams.FirstOrDefault(p => p.Key == ParamParty).Value.FirstOrDefault();

            request.AccessSubject.Add(CreateSubjectCategory(context.User.Claims));
            request.Action.Add(CreateActionCategory(requirement.ActionType));

            XacmlJsonCategory resource = CreateResourceCategoryForResource(requirement.ResourceId);
            request.Resource.Add(resource);

            if (Guid.TryParse(party, out Guid partyUuid))
            {
                resource.Attribute.Add(CreateXacmlJsonAttribute(MatchAttributeIdentifiers.PartyUuidAttribute, partyUuid.ToString(), DefaultType, DefaultIssuer));
            }
            else
            {
                throw new ArgumentException("invalid party " + party);
            }

            XacmlJsonRequestRoot jsonRequest = new() { Request = request };

            return jsonRequest;
        }

        /// <summary>
        /// Validate the response from PDP
        /// </summary>
        /// <param name="response">The response to validate</param>
        /// <param name="user">The <see cref="ClaimsPrincipal"/></param>
        /// <returns>true or false, valid or not</returns>
        public static bool ValidatePdpDecision(XacmlJsonResponse response, ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(response, nameof(response));
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            // We request one thing and then only want one result
            if (response?.Response.Count != 1)
            {
                return false;
            }

            return ValidateDecisionResult(response.Response[0], user);
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
            string from = context.Request.Query[ParamFrom];
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
            string from = context.Request.Query[ParamTo];
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
            string from = context.Request.Query[ParamParty];
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
                    Claim usersAuthenticationLevel = user.Claims.First(c => c.Type == "urn:altinn:authlevel");

                    // Checks that the user meets the minimum authentication level
                    if (Convert.ToInt32(usersAuthenticationLevel.Value) < Convert.ToInt32(minAuthenticationLevel))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static XacmlJsonAttributeAssignment GetObligation(string category, List<XacmlJsonObligationOrAdvice> obligations)
        {
            foreach (XacmlJsonObligationOrAdvice obligation in obligations)
            {
                XacmlJsonAttributeAssignment assignment = obligation.AttributeAssignment.FirstOrDefault(a => a.Category.Equals(category));
                if (assignment != null)
                {
                    return assignment;
                }
            }

            return null;
        }

        private static List<XacmlJsonAttribute> CreateSubjectAttributes(IEnumerable<Claim> claims)
        {
            List<XacmlJsonAttribute> attributes = new();

            // Mapping all claims on user to attributes
            foreach (Claim claim in claims)
            {
                if (IsSystemUserClaim(claim, out SystemUserClaim userClaim))
                {
                    attributes.Add(CreateXacmlJsonAttribute(MatchAttributeIdentifiers.SystemUserUuid, userClaim.Systemuser_id[0], DefaultType, claim.Issuer));
                }
                else if (IsUserIdClaim(claim.Type))
                {
                    attributes.Add(CreateXacmlJsonAttribute(MatchAttributeIdentifiers.UserAttribute, claim.Value, DefaultType, claim.Issuer));
                }
                else if (IsPartyUuidClaim(claim.Type))
                {
                    attributes.Add(CreateXacmlJsonAttribute(MatchAttributeIdentifiers.PartyUuidAttribute, claim.Value, DefaultType, claim.Issuer));
                }
            }

            return attributes;
        }

        private static XacmlJsonCategory CreateResourceCategoryForResource(string resourceId, bool includeResult = false)
        {
            XacmlJsonCategory resourceCategory = new XacmlJsonCategory();
            resourceCategory.Attribute =
            [
                CreateXacmlJsonAttribute(MatchAttributeIdentifiers.ResourceRegistryAttribute, resourceId, DefaultType, DefaultIssuer, includeResult),
            ];

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
            actionAttributes.Attribute =
            [
                CreateXacmlJsonAttribute(MatchAttributeIdentifiers.ActionId, actionType, DefaultType, DefaultIssuer, includeResult),
            ];
            return actionAttributes;
        }

        private static bool IsSystemUserClaim(Claim claim, out SystemUserClaim userClaim)
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

        private static bool IsPartyUuidClaim(string name)
        {
            return name.Equals(MatchAttributeIdentifiers.PartyUuidAttribute);
        }
    }
}
