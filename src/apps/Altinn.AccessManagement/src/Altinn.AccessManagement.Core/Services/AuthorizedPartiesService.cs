﻿using System.Diagnostics;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Contracts;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Profile.Models;
using Altinn.Platform.Register.Enums;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Services;

/// <inheritdoc/>
public class AuthorizedPartiesService : IAuthorizedPartiesService
{
    private readonly IContextRetrievalService _contextRetrievalService;
    private readonly IDelegationMetadataRepository _delegations;
    private readonly IAltinnRolesClient _altinnRolesClient;
    private readonly IProfileClient _profile;
    private readonly IAuthorizedPartyRepoService _authorizedPartyRepoService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedPartiesService"/> class.
    /// </summary>
    /// <param name="contextRetrievalService">Service for retrieving context information</param>
    /// <param name="delegations">Database repository for delegations</param>
    /// <param name="altinn2">SBL bridge client for role and reportee information from Altinn 2</param>
    /// <param name="profile">Service implementation for user profile retrieval</param>
    /// <param name="authorizedPartyRepoService">Service implementation for getting authorized parties from new database model</param>
    public AuthorizedPartiesService(IContextRetrievalService contextRetrievalService, IDelegationMetadataRepository delegations, IAltinnRolesClient altinn2, IProfileClient profile, IAuthorizedPartyRepoService authorizedPartyRepoService)
    {
        _contextRetrievalService = contextRetrievalService;
        _delegations = delegations;
        _altinnRolesClient = altinn2;
        _profile = profile;
        _authorizedPartyRepoService = authorizedPartyRepoService;
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedParties(BaseAttribute subjectAttribute, bool includeAltinn2, bool includeAltinn3 = true, CancellationToken cancellationToken = default) => subjectAttribute.Type switch
    {
        AltinnXacmlConstants.MatchAttributeIdentifiers.PersonId => await GetAuthorizedPartiesByPersonId(subjectAttribute.Value.ToString(), includeAltinn2, cancellationToken: cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationId => await GetAuthorizedPartiesByOrganizationId(subjectAttribute.Value, includeAltinn2, cancellationToken: cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserName => await GetAuthorizedPartiesByEnterpriseUsername(subjectAttribute.Value, includeAltinn2, cancellationToken: cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.PersonUuid => await GetAuthorizedPartiesByPersonUuid(subjectAttribute.Value, includeAltinn2, cancellationToken: cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationUuid => await GetAuthorizedPartiesByOrganizationUuid(subjectAttribute.Value, includeAltinn2, cancellationToken: cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.EnterpriseUserUuid => await GetAuthorizedPartiesByEnterpriseUserUuid(subjectAttribute.Value, includeAltinn2, cancellationToken: cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute => await GetAuthorizedPartiesByPartyId(int.Parse(subjectAttribute.Value), includeAltinn2, cancellationToken: cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute => await GetAuthorizedPartiesByUserId(int.Parse(subjectAttribute.Value), includeAltinn2, cancellationToken: cancellationToken),
        AltinnXacmlConstants.MatchAttributeIdentifiers.SystemUserUuid => await GetAuthorizedPartiesBySystemUserUuid(subjectAttribute.Value, cancellationToken),
        _ => throw new ArgumentException(message: $"Unknown attribute type: {subjectAttribute.Type}", paramName: nameof(subjectAttribute))
    };

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesByPartyId(int subjectPartyId, bool includeAltinn2, bool includeAltinn3 = true, CancellationToken cancellationToken = default)
    {
        Party subject = await _contextRetrievalService.GetPartyAsync(subjectPartyId, cancellationToken);
        if (subject?.PartyTypeName == PartyType.Person)
        {
            UserProfile user = await _profile.GetUser(new() { Ssn = subject.SSN }, cancellationToken);
            if (user != null)
            {
                return await GetAuthorizedPartiesByUserId(user.UserId, includeAltinn2, cancellationToken: cancellationToken);
            }
        }

        if (subject?.PartyTypeName == PartyType.Organisation)
        {
            return await BuildAuthorizedParties(0, subject.PartyId.SingleToList(), includeAltinn2, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesByUserId(int subjectUserId, bool includeAltinn2, bool includeAltinn3 = true, CancellationToken cancellationToken = default)
    {
        List<int> keyRoleUnits = await _contextRetrievalService.GetKeyRolePartyIds(subjectUserId, cancellationToken);
        return await BuildAuthorizedParties(subjectUserId, keyRoleUnits, includeAltinn2, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesByPersonId(string subjectNationalId, bool includeAltinn2, bool includeAltinn3 = true, CancellationToken cancellationToken = default)
    {
        UserProfile user = await _profile.GetUser(new() { Ssn = subjectNationalId }, cancellationToken);
        if (user != null)
        {
            return await GetAuthorizedPartiesByUserId(user.UserId, includeAltinn2, cancellationToken: cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesByPersonUuid(string subjectPersonUuid, bool includeAltinn2, bool includeAltinn3 = true, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(subjectPersonUuid, out Guid personUuid))
        {
            throw new ArgumentException(message: $"Not a well-formed uuid: {subjectPersonUuid}", paramName: nameof(subjectPersonUuid));
        }

        UserProfile user = await _profile.GetUser(new() { UserUuid = personUuid }, cancellationToken);
        if (user != null && user.Party.PartyTypeName == PartyType.Person)
        {
            return await GetAuthorizedPartiesByUserId(user.UserId, includeAltinn2, cancellationToken: cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesByOrganizationId(string subjectOrganizationNumber, bool includeAltinn2, bool includeAltinn3 = true, CancellationToken cancellationToken = default)
    {
        Party subject = await _contextRetrievalService.GetPartyForOrganization(subjectOrganizationNumber, cancellationToken);
        if (subject != null)
        {
            return await BuildAuthorizedParties(0, subject.PartyId.SingleToList(), includeAltinn2, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesByOrganizationUuid(string subjectOrganizationUuid, bool includeAltinn2, bool includeAltinn3 = true, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(subjectOrganizationUuid, out Guid orgUuid))
        {
            throw new ArgumentException(message: $"Not a well-formed uuid: {subjectOrganizationUuid}", paramName: nameof(subjectOrganizationUuid));
        }

        Party subject = await _contextRetrievalService.GetPartyByUuid(orgUuid, cancellationToken: cancellationToken);
        if (subject != null && subject.PartyTypeName == PartyType.Organisation)
        {
            return await BuildAuthorizedParties(0, subject.PartyId.SingleToList(), includeAltinn2, cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesByEnterpriseUsername(string subjectEnterpriseUsername, bool includeAltinn2, bool includeAltinn3 = true, CancellationToken cancellationToken = default)
    {
        UserProfile user = await _profile.GetUser(new() { Username = subjectEnterpriseUsername }, cancellationToken);
        if (user != null && user.Party.PartyTypeName == PartyType.Organisation)
        {
            return await GetAuthorizedPartiesByUserId(user.UserId, includeAltinn2, cancellationToken: cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesByEnterpriseUserUuid(string subjectEnterpriseUserUuid, bool includeAltinn2, bool includeAltinn3 = true, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(subjectEnterpriseUserUuid, out Guid enterpriseUserUuid))
        {
            throw new ArgumentException(message: $"Not a well-formed uuid: {subjectEnterpriseUserUuid}", paramName: nameof(subjectEnterpriseUserUuid));
        }

        UserProfile user = await _profile.GetUser(new() { UserUuid = enterpriseUserUuid }, cancellationToken);
        if (user != null && user.Party.PartyTypeName == PartyType.Organisation)
        {
            return await GetAuthorizedPartiesByUserId(user.UserId, includeAltinn2, cancellationToken: cancellationToken);
        }

        return await Task.FromResult(new List<AuthorizedParty>());
    }

    /// <inheritdoc/>
    public async Task<List<AuthorizedParty>> GetAuthorizedPartiesBySystemUserUuid(string subjectSystemUserUuid, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(subjectSystemUserUuid, out Guid systemUserId))
        {
            throw new ArgumentException(message: $"Not a well-formed uuid: {subjectSystemUserUuid}", paramName: nameof(subjectSystemUserUuid));
        }

        var parties = await _authorizedPartyRepoService.Get(systemUserId, cancellationToken);
        return parties.Value.ToList();
    }

    private async Task AddInstanceDelegations(List<DelegationChange> delegations, int subjectUserId, List<int> subjectPartyIds, CancellationToken cancellationToken)
    {
        var toParties = new List<Party>();
        if (subjectPartyIds?.Count > 0)
        {
            toParties.AddRange(await _contextRetrievalService.GetPartiesAsync(subjectPartyIds, false, cancellationToken) ?? []);
        }

        if (subjectUserId != 0)
        {
            var userProfile = await _profile.GetUser(new() { UserId = subjectUserId }, cancellationToken);
            if (userProfile != null)
            {
                toParties.Add(userProfile.Party);
            }
        }

        if (toParties.Count > 0)
        {
            IEnumerable<InstanceDelegationChange> instanceDelegations = await _delegations.GetAllCurrentReceivedInstanceDelegations(toParties.Select(p => (Guid)p.PartyUuid).ToList(), cancellationToken);
            var fromParties = await _contextRetrievalService.GetPartiesByUuids(instanceDelegations.Select(i => i.FromUuid), false, cancellationToken);

            foreach (var instanceDelegation in instanceDelegations)
            {
                if (fromParties.TryGetValue(instanceDelegation.FromUuid.ToString(), out Party fromParty))
                {
                    delegations.Add(new DelegationChange
                    {
                        ResourceId = instanceDelegation.ResourceId,
                        InstanceId = instanceDelegation.InstanceId,
                        FromUuidType = instanceDelegation.FromUuidType,
                        FromUuid = instanceDelegation.FromUuid,
                        OfferedByPartyId = fromParty.PartyId,
                        ToUuidType = instanceDelegation.ToUuidType,
                        ToUuid = instanceDelegation.ToUuid,
                        PerformedByUuidType = instanceDelegation.PerformedByType,
                        PerformedByUuid = instanceDelegation.PerformedBy,
                        DelegationChangeType = instanceDelegation.DelegationChangeType,
                        BlobStoragePolicyPath = instanceDelegation.BlobStoragePolicyPath,
                        BlobStorageVersionId = instanceDelegation.BlobStorageVersionId,
                        Created = instanceDelegation.Created
                    });
                }
            }
        }
    }

    private async Task<List<AuthorizedParty>> BuildAuthorizedParties(int subjectUserId, List<int> subjectPartyIds, bool includeAltinn2, CancellationToken cancellationToken)
    {
        List<AuthorizedParty> result = [];
        List<AuthorizedParty> a3AuthParties = [];
        SortedDictionary<int, AuthorizedParty> authorizedPartyDict = [];

        if (includeAltinn2 && subjectUserId != 0)
        {
            List<AuthorizedParty> a2AuthParties = await _altinnRolesClient.GetAuthorizedPartiesWithRoles(subjectUserId, cancellationToken);
            foreach (AuthorizedParty a2AuthParty in a2AuthParties)
            {
                authorizedPartyDict.Add(a2AuthParty.PartyId, a2AuthParty);
                if (a2AuthParty.Subunits != null)
                {
                    foreach (AuthorizedParty a2PartySubunit in a2AuthParty.Subunits)
                    {
                        authorizedPartyDict.Add(a2PartySubunit.PartyId, a2PartySubunit);
                    }
                }
            }

            result = a2AuthParties;
        }

        List<DelegationChange> delegations = await _delegations.GetAllDelegationChangesForAuthorizedParties(subjectUserId != 0 ? subjectUserId.SingleToList() : null, subjectPartyIds, cancellationToken: cancellationToken);
        await AddInstanceDelegations(delegations, subjectUserId, subjectPartyIds, cancellationToken);

        List<int> fromPartyIds = delegations.Select(dc => dc.OfferedByPartyId).Distinct().ToList();
        List<MainUnit> mainUnits = await _contextRetrievalService.GetMainUnits(fromPartyIds, cancellationToken);

        fromPartyIds.AddRange(mainUnits.Where(m => m.PartyId > 0).Select(m => m.PartyId.Value));
        SortedDictionary<int, Party> delegationParties = await _contextRetrievalService.GetPartiesAsSortedDictionaryAsync(fromPartyIds, true, cancellationToken);

        foreach (var delegation in delegations)
        {
            if (!authorizedPartyDict.TryGetValue(delegation.OfferedByPartyId, out AuthorizedParty authorizedParty))
            {
                // Check if offering party has a main unit / is itself a subunit. 
                MainUnit mainUnit = await _contextRetrievalService.GetMainUnit(delegation.OfferedByPartyId, cancellationToken); // Since all mainunits were retrieved earlier results are in cache.
                if (mainUnit?.PartyId > 0)
                {
                    if (authorizedPartyDict.TryGetValue(mainUnit.PartyId.Value, out AuthorizedParty mainUnitAuthParty))
                    {
                        authorizedParty = mainUnitAuthParty.Subunits.Find(p => p.PartyId == delegation.OfferedByPartyId);

                        if (authorizedParty == null)
                        {
                            if (!delegationParties.TryGetValue(delegation.OfferedByPartyId, out Party party))
                            {
                                throw new UnreachableException($"Get AuthorizedParties failed to find subunit party for an existing active delegation from OfferedByPartyId: {delegation.OfferedByPartyId}");
                            }

                            authorizedParty = new AuthorizedParty(party);
                            mainUnitAuthParty.Subunits.Add(authorizedParty);
                        }

                        authorizedPartyDict.Add(authorizedParty.PartyId, authorizedParty);
                    }
                    else
                    {
                        if (!delegationParties.TryGetValue(mainUnit.PartyId.Value, out Party mainUnitParty))
                        {
                            throw new UnreachableException($"Get AuthorizedParties failed to find mainunit party: {mainUnit.PartyId.Value} for an existing active delegation from subunit OfferedByPartyId: {delegation.OfferedByPartyId}");
                        }

                        mainUnitParty.OnlyHierarchyElementWithNoAccess = true;
                        mainUnitAuthParty = new AuthorizedParty(mainUnitParty, false);

                        // Find the authorized party as a subunit on the main unit
                        Party subunit = mainUnitParty.ChildParties.Find(p => p.PartyId == delegation.OfferedByPartyId);
                        if (subunit == null)
                        {
                            throw new UnreachableException($"Get AuthorizedParties failed to find subunit party: {delegation.OfferedByPartyId}, as child on the mainunit: {mainUnitParty.PartyId}");
                        }

                        authorizedParty = new(subunit);
                        mainUnitAuthParty.Subunits = new() { authorizedParty };
                        authorizedPartyDict.Add(mainUnitParty.PartyId, mainUnitAuthParty);
                        authorizedPartyDict.Add(authorizedParty.PartyId, authorizedParty);
                        a3AuthParties.Add(mainUnitAuthParty);
                    }
                }
                else
                {
                    // Authorized party is not a subunit. Find party to add.
                    if (!delegationParties.TryGetValue(delegation.OfferedByPartyId, out Party party))
                    {
                        throw new UnreachableException($"Get AuthorizedParties failed to find party for an existing active delegation from OfferedByPartyId: {delegation.OfferedByPartyId}");
                    }

                    authorizedParty = new AuthorizedParty(party);
                    authorizedPartyDict.Add(authorizedParty.PartyId, authorizedParty);
                    a3AuthParties.Add(authorizedParty);
                }
            }

            if (authorizedParty.OnlyHierarchyElementWithNoAccess)
            {
                // Delegation is from a MainUnit which has been added previously as hierarchy element. All children need to be added before resource enrichment
                if (!delegationParties.TryGetValue(authorizedParty.PartyId, out Party mainUnitParty))
                {
                    throw new UnreachableException($"Get AuthorizedParties failed to find mainunit party: {authorizedParty.PartyId} already added previously. Should not be possible.");
                }

                foreach (Party subunit in mainUnitParty.ChildParties)
                {
                    // Only add subunits which so far has not been already processed with some authorized access
                    if (!authorizedPartyDict.TryGetValue(subunit.PartyId, out AuthorizedParty authorizedSubUnit))
                    {
                        authorizedParty.Subunits.Add(new(subunit));
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(delegation.InstanceId))
            {
                authorizedParty.EnrichWithResourceInstanceAccess(delegation.ResourceId, delegation.InstanceId);
            }
            else
            {
                authorizedParty.EnrichWithResourceAccess(delegation.ResourceId);
            }
        }

        result.AddRange(a3AuthParties);
        return result;
    }
}
