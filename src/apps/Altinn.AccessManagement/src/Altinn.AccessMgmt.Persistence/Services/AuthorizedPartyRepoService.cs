using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessMgmt.Persistence.Services;

/// <inheritdoc/>
public class AuthorizedPartyRepoService(
    IAssignmentRepository assignmentRepository,
    IPackageRepository packageRepository,
    IAssignmentPackageRepository assignmentPackageRepository,
    IRoleRepository roleRepository,
    IRolePackageRepository rolePackageRepository,
    IEntityRepository entityRepository,
    IEntityVariantRepository entityVariantRepository,
    IDelegationRepository delegationRepository,
    IConnectionRepository connectionRepository,
    IConnectionPackageRepository connectionPackageRepository,
    IDelegationMetadataRepository resourceDelegationRepository,
    IRelationService relationService
    ) : IAuthorizedPartyRepoService
{
    private static readonly string RETTIGHETSHAVER = "rettighetshaver";
    private static readonly Guid PartyTypeOrganizationUuid = new Guid("8c216e2f-afdd-4234-9ba2-691c727bb33d");

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<AuthorizedParty>>> Get(Guid toId, CancellationToken cancellationToken = default)
    {
        Dictionary<Guid, AuthorizedParty> parties = new();
        ValidationErrorBuilder errors = default;

        var toEntity = await entityRepository.GetExtended(toId, cancellationToken: cancellationToken);
        ValidatePartyIsNotNull(toId, toEntity, ref errors, "$QUERY/to");
        ValidatePartyIsSystemUser(toEntity, ref errors, "$QUERY/to");

        if (errors.TryBuild(out var errorResult))
        {
            return errorResult;
        }

        // Get AccessPackage Delegations
        ////var connectionfilter = connectionRepository.CreateFilterBuilder();
        ////connectionfilter.Equal(t => t.ToId, toId);
        ////connectionfilter.NotSet(t => t.FromId);
        ////connectionfilter.NotSet(t => t.Id);
        ////connectionfilter.NotSet(t => t.FacilitatorId);

        ////var connections = await connectionRepository.GetExtended(connectionfilter, cancellationToken: cancellationToken);

        ////var connectionPackageFilter = connectionPackageRepository.CreateFilterBuilder();
        ////connectionPackageFilter.In(t => t.Id, connections.Select(c => c.Id));
        ////connectionPackageFilter.Equal(t => t.ToId, toId);
        ////connectionPackageFilter.NotSet(t => t.FromId);
        ////connectionPackageFilter.NotSet(t => t.PackageId);

        ////var connectionPackages = await connectionPackageRepository.GetExtended(connectionPackageFilter, cancellationToken: cancellationToken);

        var connections = await relationService.GetConnectionsTo(toId, null, null, null, cancellationToken: cancellationToken);
        ////var packages = await relationService.GetPackagesTo(toId, cancellationToken: cancellationToken);

        EnrichWithAccessPackageParties(parties, connections);

        // Get App and Resource Delegations
        List<DelegationChange> resourceDelegations = await resourceDelegationRepository.GetAllDelegationChangesForAuthorizedParties(toId.SingleToList(), cancellationToken: cancellationToken);

        var entityFilter = entityRepository.CreateFilterBuilder();
        entityFilter.In(t => t.Id, resourceDelegations.Select(d => d.FromUuid).Distinct().ToList());
        var fromEntities = await entityRepository.GetExtended(entityFilter, cancellationToken: cancellationToken);

        EnrichWithResourceParties(parties, resourceDelegations, fromEntities);

        return parties.Values;
    }

    private void EnrichWithAccessPackageParties(Dictionary<Guid, AuthorizedParty> parties, IEnumerable<RelationDto> connections)
    {
        foreach (var connection in connections)
        {
            if (!parties.TryGetValue(connection.Party.Id, out AuthorizedParty party))
            {
                party = new AuthorizedParty
                {
                    PartyUuid = connection.Party.Id,
                    Name = connection.Party.Name,
                    OrganizationNumber = connection.Party.RefId,
                    PersonId = connection.Party.RefId
                };

                parties[connection.Party.Id] = party;
            }

            var packages = connection.Packages?.Select(cp => cp.Value.ToString()).ToList() ?? [];
            party.EnrichWithAccessPackage(packages);
        }
    }

    private void EnrichWithAccessPackageParties(Dictionary<Guid, AuthorizedParty> parties, QueryResponse<ExtConnection> connections, QueryResponse<ExtConnectionPackage> connectionPackages)
    {
        foreach (var connection in connections)
        {
            if (!parties.TryGetValue(connection.From.Id, out AuthorizedParty party))
            {
                party = new AuthorizedParty
                {
                    PartyUuid = connection.From.Id,
                    Name = connection.From.Name,
                    OrganizationNumber = connection.From.RefId,
                    PersonId = connection.From.RefId
                };

                parties[connection.From.Id] = party;
            }

            var packages = connectionPackages?.Where(cp => cp.Id == connection.Id).Select(cp => cp.PackageId.ToString()).ToList() ?? [];
            if (packages.Count > 0)
            {
                party.EnrichWithAccessPackage(packages);
            }
        }
    }

    private void EnrichWithResourceParties(Dictionary<Guid, AuthorizedParty> parties, List<DelegationChange> resourceDelegations, QueryResponse<ExtEntity> fromEntities)
    {
        foreach (DelegationChange delegation in resourceDelegations)
        {
            if (!parties.TryGetValue(delegation.FromUuid.Value, out AuthorizedParty party))
            {
                var fromEntity = fromEntities.FirstOrDefault(e => e.Id == delegation.FromUuid);
                if (fromEntity == null)
                {
                    continue;
                }

                party = new AuthorizedParty
                {
                    PartyUuid = delegation.FromUuid.Value,
                    Name = fromEntity.Name,
                    OrganizationNumber = fromEntity.RefId,
                    PersonId = fromEntity.RefId
                };

                parties[delegation.FromUuid.Value] = party;
            }

            party.EnrichWithResourceAccess(delegation.ResourceId);
        }
    }

    private static void ValidatePartyIsNotNull(Guid id, ExtEntity entity, ref ValidationErrorBuilder errors, string param)
    {
        if (entity is null)
        {
            errors.Add(ValidationErrors.EntityNotExists, param, [new("partyId", id.ToString())]);
        }
    }

    private static void ValidatePartyIsSystemUser(ExtEntity entity, ref ValidationErrorBuilder errors, string param)
    {
        if (entity is not null && !entity.Type.Name.Equals("Systembruker", StringComparison.InvariantCultureIgnoreCase))
        {
            errors.Add(ValidationErrors.InvalidQueryParameter, param, [new("partyId", $"expected party of type 'SystemUser' got '{entity.Type.Name}'.")]);
        }
    }

    [DoesNotReturn]
    private static void Unreachable()
    {
        throw new UnreachableException();
    }
}
