using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;

namespace Altinn.AccessMgmt.Persistence.Services.Models;

public class NewDelegationService(
    IAssignmentRepository assignmentRepository,
    IDelegationRepository delegationRepository,
    IRoleRepository roleRepository,
    IEntityLookupRepository entityLookupRepository,
    IEntityRepository entityRepository,
    IEntityTypeRepository entityTypeRepository,
    IEntityVariantRepository entityVariantRepository,
    IProviderRepository providerRepository,
    IAssignmentPackageRepository assignmentPackageRepository,
    IPackageRepository packageRepository,
    IDelegationPackageRepository delegationPackageRepository
    )
{
    private readonly IAssignmentRepository assignmentRepository = assignmentRepository;
    private readonly IDelegationRepository delegationRepository = delegationRepository;
    private readonly IRoleRepository roleRepository = roleRepository;
    private readonly IEntityLookupRepository entityLookupRepository = entityLookupRepository;
    private readonly IEntityRepository entityRepository = entityRepository;
    private readonly IEntityTypeRepository entityTypeRepository = entityTypeRepository;
    private readonly IEntityVariantRepository entityVariantRepository = entityVariantRepository;
    private readonly IProviderRepository providerRepository = providerRepository;
    private readonly IAssignmentPackageRepository assignmentPackageRepository = assignmentPackageRepository;
    private readonly IPackageRepository packageRepository = packageRepository;
    private readonly IDelegationPackageRepository delegationPackageRepository = delegationPackageRepository;

    public async Task CreateClientDelegation(NewDelegationRequest request)
    {
        // Find user : Fredrik
        var user = await GetEntity(request.User) ?? throw new Exception(string.Format("User not found '{0}'", request.User));

        // Find Facilitator : Regnskapsfolk
        var facilitator = await GetEntity(request.Facilitator) ?? throw new Exception(string.Format("Facilitator not found '{0}'", request.Facilitator));

        // Find admin role : Tilgangstyrer eller KlientAdmin?
        var adminRole = await GetRole("TS");

        // Find user roles at facilitator : Fredrik - TS - Regnskapsfolk
        var userAssignmentFilter = assignmentRepository.CreateFilterBuilder(); // InheiritedAssign2mentRepo...
        userAssignmentFilter.Equal(t => t.FromId, facilitator.Id);
        userAssignmentFilter.Equal(t => t.ToId, user.Id);
        userAssignmentFilter.Equal(t => t.RoleId, adminRole.Id);
        var userAssignment = (await assignmentRepository.Get()).FirstOrDefault();
        if (userAssignment == null)
        {
            throw new Exception(string.Format("User '{0}' does not have '{1}' role at '{2}'", user.Name, adminRole.Name, facilitator.Name));
        }

        // Find Client Role : REGN
        var clientRole = (await roleRepository.Get(t => t.Code, request.ClientRole)).First() ?? throw new Exception(string.Format("Role not found '{0}'", request.ClientRole));

        // Find Client : Bakeriet
        var client = await GetEntity(request.Client) ?? throw new Exception(string.Format("Client not found '{0}'", request.Client));

        // Find Client Assignment : Bakeriet - (REGN) - Regnskapsfolk 
        var clientAssignment = await GetOrCreateAssignment(client, facilitator, clientRole) ?? throw new Exception(string.Format("Could not find or create assignment '{0}' - {1} - {2}", client.Name, clientRole.Code, facilitator.Name));

        // Find Agent Role : AGENT
        var agentRole = await GetRole(request.AgentRole) ?? throw new Exception(string.Format("Role not found '{0}'", request.AgentRole));

        // Find Agent
        var agent = await GetOrCreateEntity(request.Agent, request.Agent, "System", "System") ?? throw new Exception(string.Format("Could not find or create '{0}'", request.Agent));

        // Find or Create Agent Assignment : Regnskapsfolk - AGENT - SystemBruker01
        var agentAssignment = await GetOrCreateAssignment(facilitator, agent, agentRole);

        // Find or Create Delegation
        var delegationFilter = delegationRepository.CreateFilterBuilder();
        delegationFilter.Equal(t => t.FromId, client.Id);
        delegationFilter.Equal(t => t.ToId, agent.Id);
        var delegation = (await delegationRepository.Get(delegationFilter)).FirstOrDefault();
        if (delegation == null)
        {
            // NOT READY
            var res = await delegationRepository.Create(new Delegation()
            {
                Id = Guid.NewGuid(),
                FromId = client.Id,
                ToId = agent.Id
            });

            delegation = (await delegationRepository.Get(delegationFilter)).FirstOrDefault();
        }

        // Find Package (check if exists)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
        var packageFilter = packageRepository.CreateFilterBuilder();
        packageFilter.Equal(t => t.Urn, request.Package);
        var packages = await packageRepository.Get(packageFilter);
        if (packages == null || !packages.Any())
        {
            throw new Exception(string.Format("Package not found '{0}'", request.Package));
        }

        // Find AssignmentPackage
        var clientPackages = await assignmentPackageRepository.GetB(clientAssignment.Id);
        var package = clientPackages.FirstOrDefault(t => t.Urn == request.Package);
        if (package == null)
        {
            throw new Exception(string.Format("Client assignment does not have the package '{0}'", request.Package));
        }

        // Find or Create DelegationPackage
        var delegationPackageFilter = delegationPackageRepository.CreateFilterBuilder();
        delegationPackageFilter.Equal(t => t.DelegationId, delegation.Id);
        delegationPackageFilter.Equal(t => t.PackageId, package.Id);
        var delegationPackages = (await delegationPackageRepository.Get(delegationPackageFilter)).FirstOrDefault();
        if (delegationPackages == null)
        {
            var res = await delegationPackageRepository.Create(new DelegationPackage()
            {
                Id = Guid.NewGuid(),
                DelegationId = delegation.Id,
                PackageId = package.Id
            });
            delegationPackages = (await delegationPackageRepository.Get(delegationPackageFilter)).FirstOrDefault();
        }

        if (delegationPackages == null)
        {
            throw new Exception("Unable to add package to delegation");
        }

        // Happy !!!
    }

    public async Task<Entity> GetOrCreateEntity(string name, string refId, string type, string variant)
    {
        var entityType = (await entityTypeRepository.Get(t => t.Name, type)).First() ?? throw new Exception(string.Format("Type not found '{0}'", type));
        var variantFilter = entityVariantRepository.CreateFilterBuilder();
        variantFilter.Equal(t => t.TypeId, entityType.Id);
        variantFilter.Equal(t => t.Name, variant);
        var entityVariant = (await entityVariantRepository.Get(variantFilter)).First() ?? throw new Exception(string.Format("Variant not found '{0}'", type));

        var filter = entityRepository.CreateFilterBuilder();
        filter.Equal(t => t.TypeId, Guid.Empty);
        filter.Equal(t => t.VariantId, Guid.Empty);
        filter.Equal(t => t.RefId, refId);
        var res = await entityRepository.Get(filter);
        if (res != null && res.Any())
        {
            return res.First();
        }
        else
        {
            await entityRepository.Create(new Entity()
            {
                Id = Guid.NewGuid(),
                Name = name,
                RefId = refId,
                TypeId = Guid.Empty,
                VariantId = Guid.Empty
            });
        }

        return (await entityRepository.Get(filter)).FirstOrDefault();
    }

    public async Task<Assignment> GetOrCreateAssignment(Entity from, Entity to, Role role)
    {
        var filter = assignmentRepository.CreateFilterBuilder();
        filter.Equal(t => t.RoleId, role.Id);
        filter.Equal(t => t.FromId, from.Id);
        filter.Equal(t => t.ToId, to.Id);
        var clientAssignment = await assignmentRepository.Get(filter);

        if (clientAssignment != null && clientAssignment.Any())
        {
            return clientAssignment.First();
        }
        else
        {
            var roleProvider = await providerRepository.Get(role.ProviderId);
            if (roleProvider.Name != "DigDir") // Get system from token
            {
                throw new Exception(string.Format("You cannot create assignment with the role '{0}' ({1})", role.Name, role.Code));
            }

            var res = await assignmentRepository.Create(new Assignment()
            {
                Id = Guid.NewGuid(),
                FromId = from.Id,
                ToId = to.Id,
                RoleId = role.Id
            });
        }

        return (await assignmentRepository.Get(filter)).FirstOrDefault();
    }

    public async Task<Role> GetRole(string code)
    {
        return (await roleRepository.Get(t => t.Code, code)).FirstOrDefault();
    }

    public async Task<Entity> GetEntity(string lookup)
    {
        var split = lookup.Split(':');
        var lookupValue = split.Last();
        var lookupKey = lookup.Reverse().ToString().Substring(lookupValue.Length + 1).Reverse().ToString();
        var clientEntityFilter = entityLookupRepository.CreateFilterBuilder();
        clientEntityFilter.Equal(t => t.Value, lookupValue);
        clientEntityFilter.Equal(t => t.Key, lookupKey);
        var client = await entityLookupRepository.GetExtended(clientEntityFilter);
        return client.First()?.Entity ?? null;

        //// return (await entityRepository.Get(t => t.RefId, request.Client)).First() ?? throw new Exception(string.Format("Party not found '{0}'", request.Client));
    }
}

public class NewDelegationRequest
{
    public string Client { get; set; } // Baker Hansen
    public string ClientRole { get; set; } // REGN
    public string Facilitator { get; set; } // BDO
    public string Agent { get; set; } // SystemBruker-01
    public string AgentRole { get; set; } // AGENT
    public string Package { get; set; } // Regnskapsfører med signeringsrett
    public string User { get; set; } // Kjetil Nord (Admin i DBO)
}