using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Contracts;

namespace Altinn.AccessMgmt.Persistence.Services;

/// <inheritdoc/>
public class DelegationService(
    IRoleRepository roleRepository,
    IInheritedAssignmentRepository inheritedAssignmentRepository,
    IAssignmentRepository assignmentRepository,
    IAssignmentPackageRepository assignmentPackageRepository,
    IAssignmentResourceRepository assignmentResourceRepository,
    IRolePackageRepository rolePackageRepository,
    IRoleResourceRepository roleResourceRepository,
    IDelegationRepository delegationRepository,
    IPackageRepository packageRepository,
    IResourceRepository resourceRepository,
    IDelegationPackageRepository delegationPackageRepository,
    IDelegationResourceRepository delegationResourceRepository,
    IAssignmentService assignmentService
    ) : IDelegationService
{
    private readonly IRoleRepository roleRepository = roleRepository;
    private readonly IInheritedAssignmentRepository inheritedAssignmentRepository = inheritedAssignmentRepository;
    private readonly IAssignmentRepository assignmentRepository = assignmentRepository;
    private readonly IAssignmentPackageRepository assignmentPackageRepository = assignmentPackageRepository;
    private readonly IAssignmentResourceRepository assignmentResourceRepository = assignmentResourceRepository;
    private readonly IRolePackageRepository rolePackageRepository = rolePackageRepository;
    private readonly IRoleResourceRepository roleResourceRepository = roleResourceRepository;
    private readonly IDelegationRepository delegationRepository = delegationRepository;
    private readonly IPackageRepository packageRepository = packageRepository;
    private readonly IResourceRepository resourceRepository = resourceRepository;
    private readonly IDelegationPackageRepository delegationPackageRepository = delegationPackageRepository;
    private readonly IDelegationResourceRepository delegationResourceRepository = delegationResourceRepository;
    private readonly IAssignmentService assignmentService = assignmentService;

    private async Task<bool> CheckIfEntityHasRole(string roleCode, Guid fromId, Guid toId)
    {
        var role = (await roleRepository.Get(t => t.Code, roleCode)).First();

        var filter = inheritedAssignmentRepository.CreateFilterBuilder();
        filter.Equal(t => t.FromId, fromId);
        filter.Equal(t => t.ToId, toId);
        filter.Equal(t => t.RoleId, role.Id);

        var userAssignments = await assignmentRepository.Get();

        if (userAssignments == null || !userAssignments.Any())
        {
            return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public async Task<ExtDelegation> CreateDelgation(Guid userId, Guid fromAssignmentId, Guid toAssignmentId)
    {
        var fromAssignment = await assignmentRepository.GetExtended(fromAssignmentId);
        var toAssignment = await assignmentRepository.GetExtended(toAssignmentId);

        // Sjekk om from og to deler en felles entitet
        if (fromAssignment.ToId != toAssignment.FromId) 
        {
            throw new InvalidOperationException("Assignments are not connected. FromAssignment.ToId != ToAssignment.FromId");
        }

        // Sjekk om bruker er Tilgangsstyrer for FromAssignment
        // TODO: Sjekk inheireted. Man kan få TS fra DAGL
        var assResTS = await assignmentService.GetAssignment(fromAssignment.ToId, userId, "TS");
        if (assResTS == null)
        {
            throw new Exception(string.Format("User is not TS for '{0}'", fromAssignment.To.Name));
        }

        var delegation = new Delegation()
        {
            Id = Guid.NewGuid(),
            FromId = fromAssignmentId,
            ToId = toAssignmentId
        };

        var res = await delegationRepository.Create(delegation);
        if (res == 0)
        {
            throw new Exception("Failed to create delegation");
        }

        return await delegationRepository.GetExtended(delegation.Id);
    }

    /// <inheritdoc/>
    public async Task<bool> AddPackageToDelegation(Guid userId, Guid delegationId, Guid packageId)
    {
        /*
        [ ] Check i Pacakge is Delegable
        */

        /* 
        [X] Check if user is DelegationAdmin on ViaId 
        [X] Check if assignment has the package
        [X] Check if the assignment role has the package
        */

        var package = await packageRepository.Get(packageId);

        var delegation = await delegationRepository.GetExtended(delegationId);
        var fromAssignment = await assignmentRepository.GetExtended(delegation.FromId);
        var toAssignment = await assignmentRepository.GetExtended(delegation.ToId);

        var userHasTS = await CheckIfEntityHasRole("TS", toAssignment.FromId, userId);

        if (!userHasTS) 
        {
            throw new Exception($"{toAssignment.To.Name}' is not TS on '{toAssignment.From.Name}'");
        }

        var assignmentPackages = await assignmentPackageRepository.GetB(fromAssignment.Id);
        var rolePackages = await rolePackageRepository.Get(t => t.RoleId, fromAssignment.RoleId);

        if (assignmentPackages.Count(t => t.Id == packageId) == 0 && rolePackages.Count(t => t.Id == packageId) == 0) 
        {
            throw new Exception($"The source assignment does not have the package '{package.Name}'");
        }

        var res = await delegationPackageRepository.Create(new DelegationPackage() { 
            Id = Guid.NewGuid(),
            DelegationId = delegationId,
            PackageId = packageId
        });

        return res > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> AddResourceToDelegation(Guid userId, Guid delegationId, Guid resourceId)
    {
        /*
        [ ] Check i Pacakge is Delegable (?)
        [ ] Check i Resource is Delegable
        */

        /* 
        [X] Check if user is DelegationAdmin on FromId 
        [X] Check if assignment has the resource
        [X] Check if the assignment role has the resource
        [X] Check if the assignment packages has the resource
        */

        var resource = await resourceRepository.Get(resourceId);

        var delegation = await delegationRepository.GetExtended(delegationId);
        var fromAssignment = await assignmentRepository.GetExtended(delegation.FromId);
        var toAssignment = await assignmentRepository.GetExtended(delegation.ToId);

        var userHasTS = await CheckIfEntityHasRole("TS", toAssignment.FromId, userId);

        if (!userHasTS)
        {
            throw new Exception($"{toAssignment.To.Name}' is not TS on '{toAssignment.From.Name}'");
        }

        var assignmentResources = await assignmentResourceRepository.GetB(fromAssignment.Id);
        var roleResources = await roleResourceRepository.GetB(fromAssignment.RoleId);
        var rolePackages = await rolePackageRepository.Get(t => t.RoleId, fromAssignment.RoleId);
        var rolePackageResources = new Dictionary<Guid, List<Resource>>();
        foreach (var package in rolePackages)
        {
            rolePackageResources.Add(package.Id, [.. await roleResourceRepository.GetB(resourceId)]);
        }

        if (assignmentResources.Count(t => t.Id == resourceId) == 0 
            && roleResources.Count(t => t.Id == resourceId) == 0
            && rolePackageResources.SelectMany(t => t.Value).Count(t => t.Id == resourceId) == 0
            )
        {
            throw new Exception($"The source assignment does not have the resource '{resource.Name}'");
        }


        var res = await delegationResourceRepository.Create(new DelegationResource()
        {
            Id = Guid.NewGuid(),
            DelegationId = delegationId,
            ResourceId = resourceId
        });

        return res > 0;
    }
}


/*

KLIENT DELEGERINGS FLYT

Finn KlientAssignment med Role=REGN og From=BakerHansen og To=BDO
Finn eller opprett SystemBruker01 som Entity av type System hvor RefId = Uuid
Finn eller opprett AgentAssignment med Role:Agent, From:BDO og To:SystemBruker01
Opprett Delegation med KlientAssignment og AgentAssignment
Finn alle Packer på KlientAssignment som kan delegeres
Deleger en Pakke til Delegation med PakkeId og DelegeringsId
(Legg ved en constraint på hvor den kommer fra, BONUS)


StartMock:
Roller: ....
Entity: Bakeriet, Regnskapsfolk, PederAgent, GunnarLeder

 
*/
