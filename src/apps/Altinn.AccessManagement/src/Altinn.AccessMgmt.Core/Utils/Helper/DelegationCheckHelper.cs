using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessMgmt.Core.Utils.Helper
{
    public class DelegationCheckHelper
    {
        /// <summary>
        /// Check if it exist any roles giving access to the resource if there is no such access rules this must be a rule defined for the service owner as there is not any way the end user could gain access
        /// </summary>
        /// <param name="right">the right to analyze</param>
        /// <returns>the decision</returns>
        public static bool CheckIfRuleIsAnEndUserRule(Right right)
        {
            List<RightSource> rightAccessSources = right.RightSources.Where(rs => rs.RightSourceType != RightSourceType.DelegationPolicy).ToList();
            List<AttributeMatch> userAccess = [];
            if (rightAccessSources.Any())
            {
                List<AttributeMatch> roles = GetAttributeMatches(rightAccessSources.SelectMany(roleAccessSource => roleAccessSource.PolicySubjects)).FindAll(policySubject => policySubject.Id.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.RoleAttribute, StringComparison.OrdinalIgnoreCase) 
                                                                                                                                                                           || policySubject.Id.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.ExternalCcrRoleAttribute, StringComparison.OrdinalIgnoreCase)
                                                                                                                                                                           || policySubject.Id.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.ExternalCraRoleAttribute, StringComparison.OrdinalIgnoreCase)
                                                                                                                                                                           || policySubject.Id.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.AccessPackageAttribute, StringComparison.OrdinalIgnoreCase)
                                                                                                                                                                           || policySubject.Id.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.AccessPackagePersonAttribute, StringComparison.OrdinalIgnoreCase));
                return roles.Count != 0;
            }

            return false;
        }

        /// <summary>
        /// Gets the resource attribute values as out params from a Resource specified as a List of AttributeMatches
        /// </summary>
        /// <param name="input">The resource to fetch org and app from</param>
        /// <param name="resourceMatchType">the resource match type</param>
        /// <param name="resourceId">the resource id. Either a resource registry id or org/app</param>
        /// <param name="org">the org part of the resource</param>
        /// <param name="app">the app part of the resource</param>
        /// <param name="serviceCode">altinn 2 service code</param>
        /// <param name="serviceEditionCode">altinn 2 service edition code</param>
        /// <returns>A bool indicating whether params where found</returns>
        public static bool TrySplitResiurceIdIntoOrgApp(string input, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out string org, out string app)
        {
            resourceMatchType = ResourceAttributeMatchType.None;
            resourceId = null;
            org = null;
            app = null;

            if (input.StartsWith("app_", StringComparison.InvariantCultureIgnoreCase))
            {
                string[] parts = input.Split("_", 3, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 3)
                {
                    resourceMatchType = ResourceAttributeMatchType.AltinnAppId;
                    org = parts[1];
                    app = parts[2];
                    return true;
                }
            }
            else
            {
                resourceMatchType = ResourceAttributeMatchType.ResourceRegistry;
                resourceId = input;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if AccessList feature is enabled and applicable for the given right, resource, and fromParty. The AccessListMode feature is currently enabled only for orgs.
        /// </summary>
        /// <param name="right">The right to be delegated</param>
        /// <param name="resource">The resource we are making delegations for</param>
        /// <param name="fromParty">The party we are making delegations on behalf of</param>
        /// <returns>True if Access List authorization mode is enabled and applicable</returns>
        public static bool IsAccessListModeEnabledAndApplicable(Right right, ServiceResource resource, AccessManagement.Core.Models.Party.MinimalParty fromParty)
        {
            if (right.CanDelegate.HasValue && right.CanDelegate.Value
                && resource.AccessListMode == AccessManagement.Core.Enums.ResourceRegistry.ResourceAccessListMode.Enabled
                && fromParty.PartyType == EntityTypeId.Organization)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts a list of policy attribute matches into a list of attribute matches
        /// </summary>
        /// <param name="policySubjects">a list of policy attribute matches</param>
        /// <returns>a list of attribute matches</returns>
        private static List<AttributeMatch> GetAttributeMatches(IEnumerable<List<PolicyAttributeMatch>> policySubjects)
        {
            List<AttributeMatch> attributeMatches = new List<AttributeMatch>();
            foreach (List<PolicyAttributeMatch> attributeMatch in policySubjects)
            {
                attributeMatches.AddRange(attributeMatch);
            }

            return attributeMatches;
        }
    }
}
