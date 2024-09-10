using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Platform.Authorization.Configuration;
using Altinn.Platform.Authorization.Constants;
using Altinn.Platform.Authorization.Models;
using Altinn.Platform.Authorization.Models.Oed;
using Altinn.Platform.Authorization.Repositories.Interface;
using Altinn.Platform.Authorization.Services.Interface;
using Altinn.Platform.Authorization.Services.Interfaces;
using Altinn.Platform.Profile.Models;
using Altinn.Platform.Register.Models;
using Altinn.Platform.Storage.Interface.Models;
using Authorization.Platform.Authorization.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Altinn.Platform.Authorization.Services.Implementation
{
    /// <summary>
    /// The context handler is responsible for updating a context request
    /// From XACML standard
    ///
    /// Context Handler
    /// The system entity that converts decision requests in the native request format to the XACML canonical form, coordinates with Policy
    /// Information Points to add attribute values to the request context, and converts authorization decisions in the XACML canonical form to
    /// the native response format
    /// </summary>
    public class ContextHandler : IContextHandler
    {
        private readonly IInstanceMetadataRepository _policyInformationRepository;
        private readonly IRoles _rolesWrapper;
        private readonly IOedRoleAssignmentWrapper _oedRolesWrapper;
        private readonly IParties _partiesWrapper;
        private readonly IProfile _profileWrapper;
        private readonly IMemoryCache _memoryCache;
        private readonly GeneralSettings _generalSettings;
        private readonly IRegisterService _registerService;
        private readonly IPolicyRetrievalPoint _prp;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextHandler"/> class
        /// </summary>
        /// <param name="policyInformationRepository">the policy information repository handler</param>
        /// <param name="rolesWrapper">the roles handler</param>
        /// <param name="oedRolesWrapper">service handling oed role retireval</param>
        /// <param name="partiesWrapper">the party information handler</param>
        /// <param name="profileWrapper">the user profile information handler</param>
        /// <param name="memoryCache">The cache handler </param>
        /// <param name="settings">The app settings</param>
        /// <param name="registerService">Register service</param>
        /// <param name="prp">service handling policy retireval</param>
        public ContextHandler(
            IInstanceMetadataRepository policyInformationRepository, IRoles rolesWrapper, IOedRoleAssignmentWrapper oedRolesWrapper, IParties partiesWrapper, IProfile profileWrapper, IMemoryCache memoryCache, IOptions<GeneralSettings> settings, IRegisterService registerService, IPolicyRetrievalPoint prp)
        {
            _policyInformationRepository = policyInformationRepository;
            _rolesWrapper = rolesWrapper;
            _oedRolesWrapper = oedRolesWrapper;
            _partiesWrapper = partiesWrapper;
            _profileWrapper = profileWrapper;
            _memoryCache = memoryCache;
            _generalSettings = settings.Value;
            _registerService = registerService;
            _prp = prp;
        }

        /// <inheritdoc/>
        public async Task<XacmlContextRequest> Enrich(XacmlContextRequest request, bool isExternalRequest, SortedDictionary<string, AuthInfo> appInstanceInfo)
        {
            await EnrichResourceAttributes(request, isExternalRequest, appInstanceInfo);
            return await Task.FromResult(request);
        }

        /// <summary>
        /// Enriches the resource attribute collection with additional attributes retrieved based on the instance on the request
        /// </summary>
        /// <param name="request">The original Xacml Context Request</param>
        /// <param name="isExternalRequest">Defines if request comes </param>
        /// <param name="appInstanceInfo">Cache of auto info for this request</param>
        protected async Task EnrichResourceAttributes(XacmlContextRequest request, bool isExternalRequest, SortedDictionary<string, AuthInfo> appInstanceInfo)
        {
            XacmlContextAttributes resourceContextAttributes = request.GetResourceAttributes();
            XacmlResourceAttributes resourceAttributes = GetResourceAttributeValues(resourceContextAttributes);
            await EnrichResourceParty(resourceContextAttributes, resourceAttributes, isExternalRequest);

            bool resourceAttributeComplete = IsResourceComplete(resourceAttributes);

            if (!resourceAttributeComplete && !string.IsNullOrEmpty(resourceAttributes.InstanceValue))
            {
                Instance instanceData = null;
                if (!_generalSettings.UseStorageApiForInstanceAuthInfo)
                {
                    instanceData = await _policyInformationRepository.GetInstance(resourceAttributes.InstanceValue);
                }
                else
                {
                    instanceData = new();
                    
                    if (!appInstanceInfo.TryGetValue(resourceAttributes.InstanceValue, out AuthInfo authInfo))
                    {
                        authInfo = await _policyInformationRepository.GetAuthInfo(resourceAttributes.InstanceValue);
                        appInstanceInfo[resourceAttributes.InstanceValue] = authInfo;
                    }

                    instanceData.Process = authInfo.Process;
                    instanceData.AppId = authInfo.AppId;
                    instanceData.Org = instanceData.AppId.Split('/')[0];
                }

                if (instanceData != null)
                {
                    AddIfValueDoesNotExist(resourceContextAttributes, XacmlRequestAttribute.OrgAttribute, resourceAttributes.OrgValue, instanceData.Org);
                    string app = instanceData.AppId.Split("/")[1];
                    AddIfValueDoesNotExist(resourceContextAttributes, XacmlRequestAttribute.AppAttribute, resourceAttributes.AppValue, app);
                    if (instanceData.Process?.CurrentTask != null)
                    {
                        AddIfValueDoesNotExist(resourceContextAttributes, XacmlRequestAttribute.TaskAttribute, resourceAttributes.TaskValue, instanceData.Process.CurrentTask.ElementId);
                    }
                    else if (instanceData.Process?.EndEvent != null)
                    {
                        AddIfValueDoesNotExist(resourceContextAttributes, XacmlRequestAttribute.EndEventAttribute, null, instanceData.Process.EndEvent);
                    }

                    string partyId = resourceAttributes.InstanceValue.Split('/')[0];
                    AddIfValueDoesNotExist(resourceContextAttributes, XacmlRequestAttribute.PartyAttribute, resourceAttributes.ResourcePartyValue, partyId);
                    resourceAttributes.ResourcePartyValue = partyId;
                }
            }

            await EnrichSubjectAttributes(request, resourceAttributes.ResourcePartyValue, isExternalRequest);
        }

        private static bool IsResourceComplete(XacmlResourceAttributes resourceAttributes)
        {
            bool resourceAttributeComplete = false;
            if (!string.IsNullOrEmpty(resourceAttributes.OrgValue) &&
                !string.IsNullOrEmpty(resourceAttributes.AppValue) &&
                !string.IsNullOrEmpty(resourceAttributes.InstanceValue) &&
                !string.IsNullOrEmpty(resourceAttributes.ResourcePartyValue) &&
                !string.IsNullOrEmpty(resourceAttributes.TaskValue))
            {
                // The resource attributes are complete
                resourceAttributeComplete = true;
            }
            else if (!string.IsNullOrEmpty(resourceAttributes.OrgValue) &&
                !string.IsNullOrEmpty(resourceAttributes.AppValue) &&
                string.IsNullOrEmpty(resourceAttributes.InstanceValue) &&
                !string.IsNullOrEmpty(resourceAttributes.ResourcePartyValue) &&
                string.IsNullOrEmpty(resourceAttributes.TaskValue))
            {
                // The resource attributes are complete
                resourceAttributeComplete = true;
            }
            else if (!string.IsNullOrEmpty(resourceAttributes.OrgValue) &&
            !string.IsNullOrEmpty(resourceAttributes.AppValue) &&
            !string.IsNullOrEmpty(resourceAttributes.InstanceValue) &&
            !string.IsNullOrEmpty(resourceAttributes.ResourcePartyValue) &&
            !string.IsNullOrEmpty(resourceAttributes.AppResourceValue) &&
            resourceAttributes.AppResourceValue.Equals("events"))
            {
                // The resource attributes are complete
                resourceAttributeComplete = true;
            }
            else if (!string.IsNullOrEmpty(resourceAttributes.ResourceRegistryId) &&
           !string.IsNullOrEmpty(resourceAttributes.ResourcePartyValue))
            {
                // The resource attributes are complete
                resourceAttributeComplete = true;
            }

            return resourceAttributeComplete;
        }

        /// <summary>
        /// Method that adds information about the resource party 
        /// </summary>
        /// <returns></returns>
        protected async Task EnrichResourceParty(XacmlContextAttributes requestResourceAttributes, XacmlResourceAttributes resourceAttributes, bool isExternalRequest)
        {
            if (string.IsNullOrEmpty(resourceAttributes.ResourcePartyValue) && !string.IsNullOrEmpty(resourceAttributes.OrganizationNumber))
            {
                int partyId = await _registerService.PartyLookup(resourceAttributes.OrganizationNumber, null);
                if (partyId != 0)
                {
                    resourceAttributes.ResourcePartyValue = partyId.ToString();
                    requestResourceAttributes.Attributes.Add(GetPartyIdsAttribute(new List<int> { partyId }));
                }
            }
            else if (string.IsNullOrEmpty(resourceAttributes.ResourcePartyValue) && !string.IsNullOrEmpty(resourceAttributes.PersonId))
            {
                if (!isExternalRequest)
                {
                    throw new ArgumentException("Not allowed to use ssn for internal API");
                }

                int partyId = await _registerService.PartyLookup(null, resourceAttributes.PersonId);
                if (partyId != 0)
                {
                    resourceAttributes.ResourcePartyValue = partyId.ToString();
                    requestResourceAttributes.Attributes.Add(GetPartyIdsAttribute(new List<int> { partyId }));
                }
            }
        }

        /// <summary>
        /// Maps the XacmlContextAttributes for the Xacml Resource category to the Altinn XacmlResourceAttributes model
        /// </summary>
        /// <param name="resourceContextAttributes">XacmlContextAttributes for mapping of resource attribute values</param>
        /// <returns>XacmlResourceAttributes</returns>
        protected XacmlResourceAttributes GetResourceAttributeValues(XacmlContextAttributes resourceContextAttributes)
        {
            XacmlResourceAttributes resourceAttributes = new XacmlResourceAttributes();

            foreach (XacmlAttribute attribute in resourceContextAttributes.Attributes)
            {
                if (attribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.OrgAttribute))
                {
                    resourceAttributes.OrgValue = attribute.AttributeValues.First().Value;
                }

                if (attribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.AppAttribute))
                {
                    resourceAttributes.AppValue = attribute.AttributeValues.First().Value;
                }

                if (attribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.InstanceAttribute))
                {
                    resourceAttributes.InstanceValue = attribute.AttributeValues.First().Value;
                }

                if (attribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.PartyAttribute))
                {
                    resourceAttributes.ResourcePartyValue = attribute.AttributeValues.First().Value;
                }

                if (attribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.TaskAttribute))
                {
                    resourceAttributes.TaskValue = attribute.AttributeValues.First().Value;
                }

                if (attribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.AppResourceAttribute))
                {
                    resourceAttributes.AppResourceValue = attribute.AttributeValues.First().Value;
                }

                if (attribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.ResourceRegistryAttribute))
                {
                    resourceAttributes.ResourceRegistryId = attribute.AttributeValues.First().Value;
                }

                if (attribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.OrganizationNumberAttribute))
                {
                    resourceAttributes.OrganizationNumber = attribute.AttributeValues.First().Value;
                }

                if (attribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.LegacyOrganizationNumberAttribute))
                {
                    // For supporting legacy use of this attribute. (old PEPS)
                    if (string.IsNullOrEmpty(resourceAttributes.OrganizationNumber))
                    { 
                        resourceAttributes.OrganizationNumber = attribute.AttributeValues.First().Value;
                    }
                }

                if (attribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.PersonIdAttribute))
                {
                    resourceAttributes.PersonId = attribute.AttributeValues.First().Value;
                }
            }

            return resourceAttributes;
        }

        /// <summary>
        /// Add a XacmlAttribute to the resourceAttributes collection, if the existing value is empty
        /// </summary>
        /// <param name="resourceAttributes">The collection of resource attribues</param>
        /// <param name="attributeId">The attribute id</param>
        /// <param name="attributeValue">The existing attribute value</param>
        /// <param name="newAttributeValue">The new attribute value</param>
        protected void AddIfValueDoesNotExist(XacmlContextAttributes resourceAttributes, string attributeId, string attributeValue, string newAttributeValue)
        {
            if (string.IsNullOrEmpty(attributeValue))
            {
                resourceAttributes.Attributes.Add(GetAttribute(attributeId, newAttributeValue));
            }
        }

        /// <summary>
        /// Gets a XacmlAttribute model for the specified attribute id and value
        /// </summary>
        /// <param name="attributeId">The attribute id</param>
        /// <param name="attributeValue">The attribute value</param>
        /// <returns>XacmlAttribute</returns>
        protected XacmlAttribute GetAttribute(string attributeId, string attributeValue)
        {
            XacmlAttribute attribute = new XacmlAttribute(new Uri(attributeId), false);
            if (attributeId.Equals(XacmlRequestAttribute.PartyAttribute))
            {
                // When Party attribute is missing from input it is good to return it so PEP can get this information
                attribute.IncludeInResult = true;
            }

            attribute.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), attributeValue));
            return attribute;
        }

        /// <summary>
        /// Enriches the XacmlContextRequest with the Roles the subject user has for the resource reportee
        /// </summary>
        /// <param name="request">The original Xacml Context Request</param>
        /// <param name="resourceParty">The resource reportee party id</param>
        /// <param name="isExternalRequest">Used to enforce stricter requirements</param>
        protected async Task EnrichSubjectAttributes(XacmlContextRequest request, string resourceParty, bool isExternalRequest)
        {
            // If there is no resource party then it is impossible to enrich roles
            if (string.IsNullOrEmpty(resourceParty))
            {
                return;
            }

            XacmlContextAttributes subjectContextAttributes = request.GetSubjectAttributes();

            int subjectUserId = 0;
            int resourcePartyId = Convert.ToInt32(resourceParty);
            string subjectSsn = string.Empty;
            string subjectOrgnNo = string.Empty;
            bool foundLegacyOrgNoAttribute = false;

            if (subjectContextAttributes.Attributes.Any(a => a.AttributeId.OriginalString.Equals(XacmlRequestAttribute.SystemUserIdAttribute)) && subjectContextAttributes.Attributes.Count > 1)
            {
                throw new ArgumentException($"Subject attribute {XacmlRequestAttribute.SystemUserIdAttribute} can only be used by itself and not in combination with other subject identifiers.");
            }

            foreach (XacmlAttribute xacmlAttribute in subjectContextAttributes.Attributes)
            {
                if (xacmlAttribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.UserAttribute))
                {
                    subjectUserId = Convert.ToInt32(xacmlAttribute.AttributeValues.First().Value);
                }

                if (xacmlAttribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.PersonIdAttribute))
                {
                    if (!isExternalRequest)
                    {
                        throw new ArgumentException($"Not allowed to use attribute {XacmlRequestAttribute.PersonIdAttribute} for internal API");
                    }

                    subjectSsn = xacmlAttribute.AttributeValues.First().Value;
                }

                if (xacmlAttribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.LegacyOrganizationNumberAttribute))
                {
                    foundLegacyOrgNoAttribute = true;
                    subjectOrgnNo = xacmlAttribute.AttributeValues.First().Value;
                }

                if (xacmlAttribute.AttributeId.OriginalString.Equals(XacmlRequestAttribute.OrganizationNumberAttribute))
                {
                    subjectOrgnNo = xacmlAttribute.AttributeValues.First().Value;
                }
            }

            if (foundLegacyOrgNoAttribute)
            {
                subjectContextAttributes.Attributes.Add(GetOrganizationIdentifierAttribute(subjectOrgnNo));
            }

            if (!string.IsNullOrEmpty(subjectSsn) && subjectUserId != 0)
            {
                throw new ArgumentException("Not allowed to set userid and person-id for subject at the same time");
            }

            if (isExternalRequest && !string.IsNullOrEmpty(subjectOrgnNo) && (subjectUserId != 0 || !string.IsNullOrEmpty(subjectSsn)))
            {
                throw new ArgumentException("Not allowed to set organization number and person-id or userid for subject at the same time");
            }

            if (!string.IsNullOrEmpty(subjectSsn))
            {
                UserProfile subjectProfile = await _profileWrapper.GetUserProfileBySSN(subjectSsn);

                if (subjectProfile != null)
                {
                    subjectUserId = subjectProfile.UserId;
                    subjectContextAttributes.Attributes.Add(GetUserIdAttribute(subjectUserId));
                }
                else
                {
                    throw new ArgumentException("Invalid person-id");
                }
            }

            if (isExternalRequest && !string.IsNullOrEmpty(subjectOrgnNo))
            {
                int partyId = await _registerService.PartyLookup(subjectOrgnNo, null);
                subjectContextAttributes.Attributes.Add(GetPartyIdsAttribute(new List<int> { partyId }));
            }

            // No need for further enrichment of roles of no user subject exists
            if (subjectUserId == 0)
            {
                return;
            }

            XacmlPolicy xacmlPolicy = await _prp.GetPolicyAsync(request);
            if (xacmlPolicy == null)
            {
                return;
            }

            IDictionary<string, ICollection<string>> policySubjectAttributes = xacmlPolicy.GetAttributeDictionaryByCategory(XacmlConstants.MatchAttributeCategory.Subject);
            if (policySubjectAttributes.ContainsKey(AltinnXacmlConstants.MatchAttributeIdentifiers.OedRoleAttribute))
            {
                if (string.IsNullOrEmpty(subjectSsn))
                {
                    subjectSsn = await GetSsnForUser(subjectUserId);
                }
                
                string resourceSsn = await GetSSnForParty(resourcePartyId);

                if (!string.IsNullOrWhiteSpace(subjectSsn) && !string.IsNullOrWhiteSpace(resourceSsn))
                {
                    List<OedRoleAssignment> oedRoleAssignments = await GetOedRoleAssignments(resourceSsn, subjectSsn);
                    if (oedRoleAssignments.Count != 0)
                    {
                        subjectContextAttributes.Attributes.Add(GetOedRoleAttributes(oedRoleAssignments));
                    }
                }
            }

            if (policySubjectAttributes.ContainsKey(AltinnXacmlConstants.MatchAttributeIdentifiers.RoleAttribute))
            {
                List<Role> roleList = await GetRoles(subjectUserId, resourcePartyId);
                if (roleList.Count != 0)
                {
                    subjectContextAttributes.Attributes.Add(GetRoleAttribute(roleList));
                }
            }
        }

        /// <summary>
        /// Gets a XacmlAttribute model for the list of roletype codes
        /// </summary>
        /// <param name="roles">The list of roletype codes</param>
        /// <returns>XacmlAttribute</returns>
        protected XacmlAttribute GetRoleAttribute(List<Role> roles)
        {
            XacmlAttribute attribute = new XacmlAttribute(new Uri(XacmlRequestAttribute.RoleAttribute), false);
            foreach (Role role in roles)
            {
                attribute.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), role.Value));
            }

            return attribute;
        }

        /// <summary>
        /// Gets a XacmlAttribute model for the list of oed role attributes
        /// </summary>
        /// <param name="oedRoleAssignments">The list of oedRoleAssignments</param>
        /// <returns>XacmlAttribute</returns>
        protected XacmlAttribute GetOedRoleAttributes(List<OedRoleAssignment> oedRoleAssignments)
        {
            XacmlAttribute attribute = new XacmlAttribute(new Uri(XacmlRequestAttribute.OedRoleAttribute), false);
            foreach (OedRoleAssignment oedRoleAssignment in oedRoleAssignments)
            {
                attribute.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), oedRoleAssignment.OedRoleCode));
            }

            return attribute;
        }

        /// <summary>
        /// Gets a XacmlAttribute model for a list of party ids
        /// </summary>
        /// <param name="partyIds">The list of party ids</param>
        /// <returns>XacmlAttribute</returns>
        protected XacmlAttribute GetPartyIdsAttribute(List<int> partyIds)
        {
            XacmlAttribute attribute = new XacmlAttribute(new Uri(XacmlRequestAttribute.PartyAttribute), false);
            foreach (int partyId in partyIds)
            {
                attribute.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), partyId.ToString()));
            }

            return attribute;
        }

        /// <summary>
        /// Gets a XacmlAttribute model for a userId
        /// </summary>
        /// <param name="userId">UserId</param>
        /// <returns>XacmlAttribute</returns>
        protected XacmlAttribute GetUserIdAttribute(int userId)
        {
            XacmlAttribute attribute = new XacmlAttribute(new Uri(XacmlRequestAttribute.UserAttribute), false);
            attribute.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), userId.ToString()));
            return attribute;
        }

        /// <summary>
        /// Gets a XacmlAttribute model for a organization identifier (organization number)
        /// </summary>
        /// <param name="orgNo">The organization number</param>
        /// <returns>XacmlAttribute</returns>
        protected XacmlAttribute GetOrganizationIdentifierAttribute(string orgNo)
        {
            XacmlAttribute attribute = new XacmlAttribute(new Uri(XacmlRequestAttribute.OrganizationNumberAttribute), false);
            attribute.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), orgNo));
            return attribute;
        }

        /// <summary>
        /// Gets the list of roletype codes the subject user has for the resource reportee
        /// </summary>
        /// <param name="subjectUserId">The user id of the subject</param>
        /// <param name="resourcePartyId">The party id of the reportee</param>
        /// <returns>List of roles</returns>
        protected async Task<List<Role>> GetRoles(int subjectUserId, int resourcePartyId)
        {
            string cacheKey = GetCacheKey(subjectUserId, resourcePartyId);

            if (!_memoryCache.TryGetValue(cacheKey, out List<Role> roles))
            {
                // Key not in cache, so get data.
                roles = await _rolesWrapper.GetDecisionPointRolesForUser(subjectUserId, resourcePartyId) ?? new List<Role>();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _generalSettings.RoleCacheTimeout, 0));

                _memoryCache.Set(cacheKey, roles, cacheEntryOptions);
            }

            return roles;
        }

        /// <summary>
        /// Gets the list of mainunits for a subunit
        /// </summary>
        /// <param name="subUnitPartyId">The subunit partyId to check and retrieve mainunits for</param>
        /// <param name="cancellationToken">The cancellationToken</param>
        /// <returns>List of mainunits</returns>
        protected async Task<List<MainUnit>> GetMainUnits(int subUnitPartyId, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"GetMainUnitsFor:{subUnitPartyId}";

            if (!_memoryCache.TryGetValue(cacheKey, out List<MainUnit> mainUnits))
            {
                // Key not in cache, so get data.
                mainUnits = await _partiesWrapper.GetMainUnits(new MainUnitQuery { PartyIds = new List<int> { subUnitPartyId } }, cancellationToken);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _generalSettings.MainUnitCacheTimeout, 0));

                _memoryCache.Set(cacheKey, mainUnits, cacheEntryOptions);
            }

            return mainUnits;
        }

        /// <summary>
        /// Gets the list of keyrole unit partyIds for a user
        /// </summary>
        /// <param name="subjectUserId">The userid to retrieve keyrole unit for</param>
        /// <param name="cancellationToken">The cancellationToken</param>
        /// <returns>List of partyIds for units where user has keyrole</returns>
        protected async Task<List<int>> GetKeyRolePartyIds(int subjectUserId, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"GetKeyRolePartyIdsFor:{subjectUserId}";

            if (!_memoryCache.TryGetValue(cacheKey, out List<int> keyrolePartyIds))
            {
                // Key not in cache, so get data.
                keyrolePartyIds = await _partiesWrapper.GetKeyRoleParties(subjectUserId, cancellationToken);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _generalSettings.MainUnitCacheTimeout, 0));

                _memoryCache.Set(cacheKey, keyrolePartyIds, cacheEntryOptions);
            }

            return keyrolePartyIds;
        }

        /// <summary>
        /// Gets a list of role assignments between to persons (if exists) from the OED Authz PIP API
        /// </summary>
        /// <param name="from">the party which the role assignment provides access on behalf of</param>
        /// <param name="to">the role assignment recipient party</param>
        /// <returns>list of OED/Digitalt dodsbo Role Assignments</returns>
        protected async Task<List<OedRoleAssignment>> GetOedRoleAssignments(string from, string to)
        {
            string cacheKey = GetOedRoleassignmentCacheKey(from, to);

            if (!_memoryCache.TryGetValue(cacheKey, out List<OedRoleAssignment> oedRoles))
            {
                oedRoles = await _oedRolesWrapper.GetOedRoleAssignments(from, to);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _generalSettings.RoleCacheTimeout, 0));

                _memoryCache.Set(cacheKey, oedRoles, cacheEntryOptions);
            }

            return oedRoles;
        }

        private string GetCacheKey(int userId, int partyId)
        {
            return "rolelist_" + userId + "_" + partyId;
        }

        private string GetOedRoleassignmentCacheKey(string from, string to)
        {
            return $"oed{from}_{to}";
        }

        private async Task<string> GetSsnForUser(int userId)
        {
            string cacheKey = $"uid:{userId}";

            if (!_memoryCache.TryGetValue(cacheKey, out UserProfile userProfile))
            {
                userProfile = await _profileWrapper.GetUserProfile(userId);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _generalSettings.RoleCacheTimeout, 0));

                _memoryCache.Set(cacheKey, userProfile, cacheEntryOptions);
            }

            return userProfile?.Party?.SSN;
        }

        private async Task<string> GetSSnForParty(int partyId)
        {
            string cacheKey = $"p:{partyId}";

            if (!_memoryCache.TryGetValue(cacheKey, out Party party))
            {
                party = await _partiesWrapper.GetParty(partyId);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _generalSettings.RoleCacheTimeout, 0));

                _memoryCache.Set(cacheKey, party, cacheEntryOptions);
            }

            return party?.SSN;
        }
    }
}
