using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Altinn.Authorization.Workers.BrReg.Models;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.Workers.BrReg.Services;

/// <summary>
/// Ingest initial data from Brreg API
/// </summary>
/// <param name="config">IngestorConfig</param>
/// <param name="entityService">IEntityService</param>
/// <param name="assignmentService">IAssignmentService</param>
/// <param name="entityTypeService">IEntityTypeService</param>
/// <param name="entityVariantService">IEntityVariantService</param>
/// <param name="roleService">IRoleService</param>
public class Ingestor(IOptions<IngestorConfig> config, IEntityService entityService, IAssignmentService assignmentService, IEntityTypeService entityTypeService, IEntityVariantService entityVariantService, IRoleService roleService)
{
    private IngestorConfig Config { get; } = config.Value;

    private BrregApiWrapper BrregApi { get; set; } = new BrregApiWrapper();

    private IEntityService EntityService { get; } = entityService;

    private IAssignmentService AssignmentService { get; } = assignmentService;

    private IEntityTypeService EntityTypeService { get; } = entityTypeService;

    private IEntityVariantService EntityVariantService { get; } = entityVariantService;

    private IRoleService RoleService { get; } = roleService;

    /// <summary>
    /// Ingest Units, SubUnits and Roles
    /// </summary>
    /// <returns></returns>
    public async Task IngestAll(bool force = false)
    {
        ////using var a = MyTelemetry.Source.StartActivity("IngestAll");
        ////a?.AddEvent(new System.Diagnostics.ActivityEvent("Ingest starting!"));

        await LoadCache();

        if (EntityIdCache.Count == 0 || force)
        {
            await IngestUnits();
            await LoadCache();
            await IngestSubUnits();
        }
        else
        {
            Console.WriteLine("Entities in db. Abort ingest");
        }

        if (CacheRole.Count == 0 || force)
        {
            await LoadCache();
            await IngestRoles();
        }
        else
        {
            Console.WriteLine("Roles in db. Abort ingest");
        }
    }

    /// <summary>
    /// Ingest Units
    /// </summary>
    /// <returns></returns>
    public async Task IngestUnits()
    {
        ////using var a = MyTelemetry.Source.StartActivity("IngestUnits");

        ////a?.AddEvent(new System.Diagnostics.ActivityEvent("Getting units"));
        var units = await BrregApi.GetAllUnits();

        ////a?.AddEvent(new System.Diagnostics.ActivityEvent("Converting units to entities"));
        var entities = units.Select(GenerateOrgEntity);

        ////a?.AddEvent(new System.Diagnostics.ActivityEvent("Writing entities to Db"));
        await EntityService.Repo.Ingest(entities.OfType<Entity>().ToList());
    }

    /// <summary>
    /// Ingest SubUnits
    /// </summary>
    /// <returns></returns>
    public async Task IngestSubUnits()
    {
        Console.WriteLine("Getting subunits");
        var units = await BrregApi.GetAllSubUnits();

        Console.WriteLine($"Cleaning units ({units.Count})");
        var cleanUnits = units.Where(t => !EntityIdCache.ContainsKey(t.OrgNo));

        Console.WriteLine($"Converting subunits ({cleanUnits.Count()}) to entities");
        var entities = cleanUnits.Select(GenerateOrgEntity);

        Console.WriteLine($"Writing entities ({entities.Count()}) to Db");
        await EntityService.Repo.Ingest(entities.OfType<Entity>().ToList());
    }

    /// <summary>
    /// Ingest Roles
    /// </summary>
    /// <returns></returns>
    public async Task IngestRoles()
    {
        Console.WriteLine("Getting roles");
        var roleResults = await BrregApi.GetAllUnitRoles();

        Console.WriteLine("Generate People Entity");
        var people = roleResults.SelectMany(res => res.RoleGroups).SelectMany(roleGrp => roleGrp.Roles).Where(role => role.Person != null).Select(p => p.Person).Distinct().ToList();
        var prePeopleEntities = people.Select(GeneratePersonEntity).OfType<Entity>().DistinctBy(t => t.Name + t.TypeId.ToString() + t.RefId).Where(t => !EntityIdCache.ContainsKey(t.RefId)).ToList();
        Console.WriteLine($"Ingest People ({prePeopleEntities.Count})");
        await EntityService.Repo.Ingest([.. prePeopleEntities]);

        await LoadCache();

        var orgs1 = roleResults.SelectMany(res => res.RoleGroups).SelectMany(roleGrp => roleGrp.Roles).Where(role => role.Organization != null).Select(p => p.Organization.OrgNo).Distinct().ToList();
        var orgs2 = roleResults.Select(res => res.OrgNo).Distinct().ToList();

        orgs1.AddRange(orgs2);
        var orgs = orgs1.Distinct();

        Console.WriteLine($"Orgs: {orgs.Count()}");
        var orgsClean = orgs.Where(t => !EntityIdCache.ContainsKey(t)).ToList();
        Console.WriteLine($"Clean Orgs: {orgsClean.Count}");

        Console.WriteLine("Converting roles to assignments");
        var assignments = new List<Assignment>();
        foreach (var r in roleResults)
        {
            assignments.AddRange(GenerateAssignmentsFast(r) ?? []);
        }

        /*Retry*/
        Console.WriteLine("Running retry");
        Console.WriteLine($"Retry status:\tRoleOrgQueue:{RoleOrgQueue.Count}\tRoleEntityQueue:{RoleEntityQueue.Count}");
        var failedOrgRetry = new List<string>();
        foreach (var org in RoleOrgQueue)
        {
            try
            {
                var unit = await BrregApi.GetUnit(org) ?? throw new Exception($"Unit not found '{org}'");
                var entity = GenerateOrgEntity(unit) ?? throw new Exception($"Unable to generate Entity from Unit '{org}'");
                RoleEntityQueue.Add(entity);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                failedOrgRetry.Add(org);
            }
        }

        Console.WriteLine($"Retry status:\tRoleOrgQueue:{RoleOrgQueue.Count}\tRoleEntityQueue:{RoleEntityQueue.Count}");
        var failedOrgSecondRetry = new List<string>();
        foreach (var org in failedOrgRetry)
        {
            try
            {
                var unit = await BrregApi.GetSubUnit(org) ?? throw new Exception($"SubUnit not found '{org}'");
                var entity = GenerateOrgEntity(unit) ?? throw new Exception($"Unable to generate Entity from SubUnit '{org}'");
                RoleEntityQueue.Add(entity);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                failedOrgSecondRetry.Add(org);
            }
        }

        Console.WriteLine($"Retry status:\tRoleOrgQueue:{RoleOrgQueue.Count}\tRoleEntityQueue:{RoleEntityQueue.Count}");
        try
        {
            if (RoleEntityQueue != null && RoleEntityQueue.Count > 0)
            {
                await EntityService.Repo.Ingest(RoleEntityQueue);
                RoleEntityQueue.Clear();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        var failedEntityRetry = new List<Entity>();
        if (RoleEntityQueue != null && RoleEntityQueue.Count > 0)
        {
            Console.WriteLine($"Retry status:\tRoleOrgQueue:{RoleOrgQueue.Count}\tRoleEntityQueue:{RoleEntityQueue.Count}");
            foreach (var entity in RoleEntityQueue)
            {
                try
                {
                    await EntityService.Repo.Create(entity);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create Entity '{entity.RefId}'. " + ex.Message);
                    failedEntityRetry.Add(entity);
                }
            }
        }

        if (failedOrgSecondRetry.Count > 0 && failedEntityRetry.Count > 0 && RoleResultRetryQueue.Count > 0)
        {
            Console.WriteLine($"failedOrgSecondRetry: {failedOrgSecondRetry.Count}");
            Console.WriteLine($"failedEntityRetry: {failedEntityRetry.Count}");
            Console.WriteLine($"RoleResultRetryQueue: {RoleResultRetryQueue.Count}");

            // Console.WriteLine("Continue?");
            // Console.ReadLine();
        }
        else
        {
            Console.WriteLine("ALL GOOD!");
        }

        Console.WriteLine($"Writing assignments ({assignments.Count}) to Db");
        await AssignmentService.Repo.Ingest(assignments.OfType<Assignment>().DistinctBy(t => t.ToId.ToString() + t.FromId.ToString() + t.RoleId.ToString()).ToList());
    }

    #region Queue
    private List<Entity> RoleEntityQueue { get; set; } = [];

    private List<string> RoleOrgQueue { get; set; } = [];

    private void QueueRoleEntity(Entity? entity)
    {
        if (entity != null && !RoleEntityQueue.Any(t => t.RefId == entity.RefId) && !RoleOrgQueue.Any(t => t == entity.RefId))
        {
            RoleEntityQueue.Add(entity);
        }
    }

    private void QueueRoleEntity(string orgNo)
    {
        if (!RoleEntityQueue.Any(t => t.RefId == orgNo) && !RoleOrgQueue.Any(t => t == orgNo))
        {
            RoleOrgQueue.Add(orgNo);
        }
    }
    #endregion

    #region Generate Assignment
    private List<RoleResult> RoleResultRetryQueue { get; set; } = [];

    /// <summary>
    /// Generate Assignments based on RoleResult
    /// NOT Fast: Will check database for missing information
    /// </summary>
    /// <param name="roleResult">RoleResult</param>
    /// <returns></returns>
    private List<Assignment> GenerateAssignments(RoleResult roleResult)
    {
        bool hasErrors = false;

        var result = new List<Assignment>();
        var forEntityId = LookupEntityId(roleResult.OrgNo);
        if (!forEntityId.HasValue)
        {
            QueueRoleEntity(roleResult.OrgNo);
            hasErrors = true;
        }

        foreach (var roleGroup in roleResult.RoleGroups)
        {
            foreach (var role in roleGroup.Roles)
            {
                Guid? toEntityId = null;
                if (role.Person != null)
                {
                    toEntityId = LookupEntityId(GeneratePersonRefId(role.Person));
                    if (toEntityId == null)
                    {
                        QueueRoleEntity(GeneratePersonEntity(role.Person));
                        hasErrors = true;
                    }
                }

                if (role.Organization != null)
                {
                    toEntityId = LookupEntityId(role.Organization.OrgNo);
                    if (toEntityId == null)
                    {
                        QueueRoleEntity(role.Organization.OrgNo);
                        hasErrors = true;
                    }
                }

                var entityRole = LookupRole(role.Type.Code);

                if (entityRole != null && toEntityId.HasValue && forEntityId.HasValue)
                {
                    result.Add(new Assignment()
                    {
                        Id = Guid.NewGuid(),
                        FromId = forEntityId.Value,
                        RoleId = entityRole.Id,
                        ToId = toEntityId.Value
                    });
                }
            }
        }

        if (hasErrors)
        {
            RoleResultRetryQueue.Add(roleResult);
            return [];
        }
        else
        {
            return result;
        }
    }

    /// <summary>
    /// Generate Assignments based on RoleResult
    /// Fast: Will not check database for missing information
    /// </summary>
    /// <param name="roleResult">RoleResult</param>
    /// <returns></returns>
    private List<Assignment> GenerateAssignmentsFast(RoleResult roleResult)
    {
        bool hasErrors = false;
        var result = new List<Assignment>();

        try
        {
            var forEntityId = EntityIdCache[roleResult.OrgNo];
            foreach (var roleGroup in roleResult.RoleGroups)
            {
                foreach (var role in roleGroup.Roles)
                {
                    Guid? toEntityId = null;
                    if (role.Person != null)
                    {
                        toEntityId = EntityIdCache[GeneratePersonRefId(role.Person)];
                    }

                    if (role.Organization != null)
                    {
                        toEntityId = EntityIdCache[role.Organization.OrgNo];
                    }

                    var entityRole = LookupRole(role.Type.Code);

                    if (entityRole != null && toEntityId.HasValue)
                    {
                        result.Add(new Assignment()
                        {
                            Id = Guid.NewGuid(),
                            FromId = forEntityId,
                            RoleId = entityRole.Id,
                            ToId = toEntityId.Value
                        });
                    }
                }
            }
        }
        catch
        {
            hasErrors = true;
        }

        if (hasErrors)
        {
            RoleResultRetryQueue.Add(roleResult);
            return [];
        }
        else
        {
            return result;
        }
    }
    #endregion

    #region Generate Entity
    private Entity? GeneratePersonEntity(RolePerson person)
    {
        return GeneratePersonEntity(GeneratePersonDisplayName(person), GeneratePersonRefId(person));
    }

    private Entity? GeneratePersonEntity(string name, string refId)
    {
        var typeVariant = LookupTypeVariant("Person", "Person");
        if (typeVariant.Type == null || typeVariant.Variant == null)
        {
            return null;
        }

        return new Entity()
        {
            Id = Guid.NewGuid(),
            Name = name,
            RefId = refId,
            TypeId = typeVariant.Type.Id,
            VariantId = typeVariant.Variant.Id
        };
    }

    private string GeneratePersonDisplayName(RolePerson person)
    {
        var nameParts = new string[] { person.Name.Firstname, person.Name.Middlename, person.Name.Lastname };
        return string.Join(' ', nameParts.Where(t => !string.IsNullOrEmpty(t)));
    }

    private string GeneratePersonRefId(RolePerson person)
    {
        var str = $"{person.Birthdate}{person.Name.Lastname}{person.Name.Firstname}{person.Name.Middlename}"; // RemoveSpaces?
        if (str.Length > 50)
        {
            return str[..50];
        }

        return str;
    }

    private Entity? GenerateOrgEntity(Unit unit)
    {
        return GenerateOrgEntity(unit.Name, unit.OrgNo, unit.OrgForm.Code);
    }

    private Entity? GenerateOrgEntity(string name, string orgNo, string orgForm)
    {
        var typeVariant = LookupTypeVariant("Organisasjon", orgForm);
        if (typeVariant.Type == null || typeVariant.Variant == null)
        {
            return null;
        }

        return new Entity()
        {
            Id = Guid.NewGuid(),
            Name = name,
            RefId = orgNo,
            TypeId = typeVariant.Type.Id,
            VariantId = typeVariant.Variant.Id
        };
    }
    #endregion

    #region Cache

    /// <summary>
    /// Loads cache for lookups
    /// </summary>
    /// <returns></returns>
    private async Task LoadCache()
    {
        Console.WriteLine("Loading cache...");

        CacheEntityType = [.. await EntityTypeService.Repo.Get()];
        ////BrregIngestMetrics.EntityTypeCacheCounter.Add(CacheEntityType.Count);

        CacheEntityVariant = [.. await EntityVariantService.Repo.Get()];
        ////BrregIngestMetrics.EntityVariantCacheCounter.Add(CacheEntityVariant.Count);

        CacheRole = [.. await RoleService.Repo.Get()];
        ////BrregIngestMetrics.RoleCacheCounter.Add(CacheRole.Count);

        var res = await EntityService.Repo.Get();
        EntityIdCache = res.ToDictionary(k => k.RefId, v => v.Id);
        ////BrregIngestMetrics.EntityIdCacheCounter.Add(EntityIdCache.Count);

        Console.WriteLine($"CacheEntityType:{CacheEntityType.Count}\t" +
            $"CacheEntityVariant:{CacheEntityVariant.Count}\t" +
            $"CacheRole:{CacheRole.Count}\t" +
            $"EntityIdCache:{EntityIdCache.Count}\t");
    }

    private List<EntityType> CacheEntityType { get; set; } = [];

    private List<EntityVariant> CacheEntityVariant { get; set; } = [];

    private List<Role> CacheRole { get; set; } = [];

    private Dictionary<string, Guid> EntityIdCache { get; set; } = [];

    #endregion

    #region Lookup
    private (EntityType? Type, EntityVariant? Variant) LookupTypeVariant(string typeName, string variantName)
    {
        var type = LookupType(typeName);
        if (type == null)
        {
            return (null, null);
        }

        return (type, LookupVariant(type.Id, variantName));
    }

    private EntityType? LookupType(string name)
    {
        return CacheEntityType.FirstOrDefault(t => t.Name == name);
    }

    private EntityVariant? LookupVariant(Guid typeId, string name)
    {
        return CacheEntityVariant.FirstOrDefault(t => t.TypeId == typeId && t.Name == name);
    }

    private Role? LookupRole(string code)
    {
        return CacheRole.FirstOrDefault(t => t.Code == code);
    }

    private Guid? LookupEntityId(string refIf)
    {
        return EntityIdCache.FirstOrDefault(t => t.Key == refIf).Value;
    }


    #endregion
}
