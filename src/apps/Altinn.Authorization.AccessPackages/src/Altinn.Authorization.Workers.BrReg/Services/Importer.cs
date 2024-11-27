using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Altinn.Authorization.Workers.BrReg.Models;

namespace Altinn.Authorization.Workers.BrReg.Services;

/// <summary>
/// Import changes from Brreg API
/// </summary>
public class Importer
{
    #region Constructor
    private BrregApiWrapper BrregApi { get; set; }

    private IEntityService EntityService { get; set; }

    private IEntityTypeService EntityTypeService { get; }

    private IEntityVariantService EntityVariantService { get; }

    private IRoleAssignmentService RoleAssignmentService { get; }

    private IRoleService RoleService { get; }

    /// <summary>
    /// Importer
    /// </summary>
    /// <param name="entityService">IEntityService</param>
    /// <param name="entityTypeService">IEntityTypeService</param>
    /// <param name="entityVariantService">IEntityVariantService</param>
    /// <param name="roleAssignmentService">IRoleAssignmentService</param>
    /// <param name="roleService">IRoleService</param>
    public Importer(
       IEntityService entityService,
       IEntityTypeService entityTypeService,
       IEntityVariantService entityVariantService,
       IRoleAssignmentService roleAssignmentService,
       IRoleService roleService
       )
    {
        BrregApi = new BrregApiWrapper();
        EntityService = entityService;
        EntityTypeService = entityTypeService;
        EntityVariantService = entityVariantService;
        RoleAssignmentService = roleAssignmentService;
        RoleService = roleService;
        ChangeRef = new Dictionary<string, int>
        {
            { "enhet", 0 },
            { "underenhet", 0 },
            { "roller", 0 }
        };
    }
    #endregion

    #region ChangeRef
    private Dictionary<string, int> ChangeRef { get; set; }

    private int GetChangeId(string type)
    {
        return ChangeRef[type];
    }

    private void UpdateChangeId(string type, int id)
    {
        Console.WriteLine(type + ":" + id);
        if (id > GetChangeId(type))
        {
            ChangeRef[type] = id;
        }
    }

    /// <summary>
    /// Write ChangeIds to console
    /// </summary>
    public void WriteChangeRefsToConsole()
    {
        foreach (var changeRef in ChangeRef)
        {
            Console.WriteLine($"{changeRef.Key}:{changeRef.Value}");
        }
    }
    #endregion

    #region Cache
    private async Task LoadCache()
    {
        Console.WriteLine("Loading cache...");
        CacheEntityType = [.. await EntityTypeService.Get()];
        CacheEntityVariant = [.. await EntityVariantService.Get()];
        CacheRole = [.. await RoleService.Get()];

        var res = await EntityService.Get();
        EntityIdCache = res.ToDictionary(k => k.RefId, v => v.Id);

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
    private Role? LookupRole(string name)
    {
        return CacheRole.FirstOrDefault(t => t.Code == name) ?? throw new Exception($"Role not found '{name}'");
    }

    private EntityType? LookupType(string typeName)
    {
        return CacheEntityType.FirstOrDefault(t => t.Name == typeName);
    }

    private EntityVariant? LookupVariant(string typeName, string variantName)
    {
        var type = LookupType(typeName);
        if (type == null)
        {
            return null;
        }

        return CacheEntityVariant.FirstOrDefault(t => t.TypeId == type.Id && t.Name == variantName);
    }

    private EntityVariant? LookupVariant(Guid typeId, string variantName)
    {
        return CacheEntityVariant.FirstOrDefault(t => t.TypeId == typeId && t.Name == variantName);
    }

    private (EntityType? Type, EntityVariant? Variant) LookupTypeVariant(string typeName, string variantName)
    {
        return (LookupType(typeName), LookupVariant(typeName, variantName));
    }

    #endregion

    #region Generate
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

    private string GeneratePersonRefId(RolePerson person)
    {
        var str = $"{person.Birthdate}{person.Name.Lastname}{person.Name.Firstname}{person.Name.Middlename}";
        if (str.Length > 50)
        {
            return str[..50];
        }

        return str;
    }

    private string GeneratePersonDisplayName(RolePerson person)
    {
        var nameParts = new string[] { person.Name.Firstname, person.Name.Middlename, person.Name.Lastname };
        return string.Join(' ', nameParts.Where(t => !string.IsNullOrEmpty(t)));
    }

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
    #endregion

    #region UnitChanges

    /// <summary>
    /// Import Unit changes
    /// </summary>
    /// <returns></returns>
    public async Task ImportUnit()
    {
        Console.WriteLine("Importing unit changes");
        await LoadCache();
        var changeId = GetChangeId("enhet");
        if (changeId == 0)
        {
            var sinceDate = DateTime.Parse(DateTime.Now.ToShortDateString());
            if (sinceDate.DayOfWeek == DayOfWeek.Sunday)
            {
                /*
                 Brreg Bug ...
                 */
                sinceDate.AddDays(-1);
            }

            Console.WriteLine($"Getting changes since {sinceDate}");
            var changes = await BrregApi.GetUnitChanges(sinceDate: sinceDate);

            Console.WriteLine($"Handeling changes ({changes.Count})");
            foreach (var change in changes)
            {
                await HandleUnitChange(change, isSubUnit: false);
                UpdateChangeId("enhet", change.ChangeId);
            }
        }
        else
        {
            Console.WriteLine($"Getting changes since {changeId}");
            var changes = await BrregApi.GetUnitChanges(changeId: changeId);
            Console.WriteLine($"Handeling changes ({changes.Count})");
            foreach (var change in changes)
            {
                await HandleUnitChange(change, isSubUnit: false);
                UpdateChangeId("enhet", change.ChangeId);
            }
        }

        Console.WriteLine("Role import complete");
    }

    /// <summary>
    /// Import SubUnit changes
    /// </summary>
    /// <returns></returns>
    public async Task ImportSubUnit()
    {
        var changeId = GetChangeId("underenhet");

        if (changeId == 0)
        {
            var sinceDate = DateTime.Parse(DateTime.Now.ToShortDateString());
            if (sinceDate.DayOfWeek == DayOfWeek.Sunday)
            {
                sinceDate.AddDays(-1);
            }

            Console.WriteLine($"Getting changes since {sinceDate}");
            var changes = await BrregApi.GetSubUnitChanges(sinceDate: sinceDate);
            Console.WriteLine($"Handeling changes ({changes.Count})");
            foreach (var change in changes)
            {
                await HandleUnitChange(change, isSubUnit: true);
                UpdateChangeId("underenhet", change.ChangeId);
            }
        }
        else
        {
            Console.WriteLine($"Getting changes since {changeId}");
            var changes = await BrregApi.GetSubUnitChanges(changeId: changeId);
            Console.WriteLine($"Handeling changes ({changes.Count})");
            foreach (var change in changes)
            {
                await HandleUnitChange(change, isSubUnit: true);
                UpdateChangeId("underenhet", change.ChangeId);
            }
        }
    }

    private async Task HandleUnitChange(UnitChange change, bool isSubUnit)
    {
        try
        {
            switch (change.ChangeType)
            {
                case "Ny":
                    await CreateUnitEnity(change.OrgNo, isSubUnit);
                    break;
                case "Endring":
                case "Ukjent":
                    await UpdateUnitEntity(change.OrgNo, isSubUnit);
                    break;
                case "Sletting":
                case "Fjernet":
                    await DeleteUnitEntity(change.OrgNo);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to implement '{change.ChangeType}' change #{change.ChangeId} for '{change.OrgNo}'");
            Console.WriteLine(ex.Message);
        }
    }

    private async Task CreateUnitEnity(string orgNo, bool isSubUnit)
    {
        var unit = await GetUnit(orgNo, isSubUnit);
        if (unit == null)
        {
            return;
        }

        var entity = GenerateOrgEntity(unit.Name, unit.OrgNo, unit.OrgForm.Code);
        if (entity == null)
        {
            return;
        }

        await EntityService.Create(entity);
    }

    private async Task UpdateUnitEntity(string orgNo, bool isSubUnit)
    {
        var unit = await GetUnit(orgNo, isSubUnit);
        if (unit == null)
        {
            return;
        }

        var type = LookupType("Organisasjon");
        if (type == null)
        {
            return;
        }

        var entity = await EntityService.GetByRefId(orgNo, type.Id);
        if (entity == null)
        {
            return;
        }

        if (unit.Name != entity.Name)
        {
            await EntityService.Update(entity.Id, "Name", unit.Name);
        }
    }

    private async Task DeleteUnitEntity(string orgNo)
    {
        var type = LookupType("Organisasjon");
        if (type == null)
        {
            return;
        }

        var entity = await EntityService.GetByRefId(orgNo, type.Id);
        if (entity == null)
        {
            return;
        }

        await EntityService.Repo.Delete(entity.Id);
    }

    private async Task<Unit?> GetUnit(string orgNo, bool isSubUnit)
    {
        if (isSubUnit)
        {
            return await BrregApi.GetSubUnit(orgNo);
        }
        else
        {
            return await BrregApi.GetUnit(orgNo);
        }
    }
    #endregion

    #region RoleChanges

    /// <summary>
    /// Import Role changes
    /// </summary>
    /// <returns></returns>
    public async Task ImportRoles()
    {
        var changeId = GetChangeId("roller");
        Console.WriteLine("Getting role changes");

        if (changeId == 0)
        {
            var sinceDate = DateTime.Parse(DateTime.Now.ToShortDateString());
            if (sinceDate.DayOfWeek == DayOfWeek.Sunday)
            {
                /*
                 Brreg bug?
                 */

                sinceDate.AddDays(-1);
            }

            Console.WriteLine($"Getting changes since {sinceDate}");
            var changes = await BrregApi.GetAllRoleChanges(sinceDate: sinceDate);
            Console.WriteLine($"Handeling changes ({changes.Count})");
            foreach (var change in changes)
            {
                await ChangeRole(change);
                UpdateChangeId("roller", change.Id);
            }
        }
        else
        {
            Console.WriteLine($"Getting changes since {changeId}");
            var changes = await BrregApi.GetAllRoleChanges(changeId);
            Console.WriteLine($"Handeling changes ({changes.Count})");
            foreach (var change in changes)
            {
                await ChangeRole(change);
                UpdateChangeId("roller", change.Id);
            }
        }

        Console.WriteLine("RoleAssignment - Done");
    }

    /// <summary>
    /// RoleAssignment Comparer
    /// </summary>
    internal class RoleAssignmentComparer
    {
        /// <summary>
        /// ToId + ForId + RoleId
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// RoleAssignment
        /// </summary>
        public RoleAssignment RoleAssignment { get; set; }

        /// <summary>
        /// RoleAssignmentComparer
        /// </summary>
        /// <param name="roleAssignment">RoleAssignment</param>
        public RoleAssignmentComparer(RoleAssignment roleAssignment)
        {
            RoleAssignment = roleAssignment;
            Key = RoleAssignment.ToId.ToString() + RoleAssignment.ForId.ToString() + RoleAssignment.RoleId.ToString();
        }

        /// <summary>
        /// Key
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Key;
        }
    }

    private async Task ChangeRole(RoleChange change)
    {
        try
        {
            Console.WriteLine("RoleChangeType:" + change.Type + " for " + change.Data.OrgNo);
            var roleResult = await BrregApi.GetUnitRoles(change.Data.OrgNo);
            if (roleResult == null)
            {
                return;
            }

            var entityId = EntityIdCache[change.Data.OrgNo];

            Console.WriteLine($"Generate new assignments from result {roleResult.RoleGroups.Count}");
            var (result, hasErrors) = GenerateRoleAssignments(roleResult, change.Data.OrgNo);

            /*
             Do something with .hasErrors
             */

            Console.WriteLine("Getting old for assignments");
            var oldForAssignments = await RoleAssignmentService.Get(t => t.ForId, entityId);

            if (result == null || oldForAssignments == null)
            {
                return;
            }

            var newAssList = result.Select(t => new RoleAssignmentComparer(t)).ToList();
            var oldAssList = oldForAssignments.Select(t => new RoleAssignmentComparer(t)).ToList();

            if (newAssList == null || oldAssList == null)
            {
                return;
            }

            Console.WriteLine($"New: {newAssList.Count} Old:{oldAssList.Count}");

            var ingest = newAssList.ExceptBy(oldAssList.Select(t => t.Key), t => t.Key);
            var remove = oldAssList.ExceptBy(newAssList.Select(t => t.Key), t => t.Key);

            Console.WriteLine($"Ingest: {ingest.Count()} Remove:{remove.Count()}");

            foreach (var i in ingest)
            {
                await RoleAssignmentService.Create(i.RoleAssignment);
            }

            foreach (var r in remove)
            {
                await RoleAssignmentService.Delete(r.RoleAssignment.Id);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }
    }

    private (List<RoleAssignment> Result, bool HasErrors) GenerateRoleAssignments(RoleResult roleResult, string refId)
    {
        bool hasErrors = false;
        var result = new List<RoleAssignment>();

        try
        {
            Guid forEntityId = EntityIdCache[refId];

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
                        result.Add(new RoleAssignment()
                        {
                            Id = Guid.NewGuid(),
                            ForId = forEntityId,
                            RoleId = entityRole.Id,
                            ToId = toEntityId.Value
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            hasErrors = true;
        }

        if (hasErrors)
        {
            return (new List<RoleAssignment>(), true);
        }
        else
        {
            return (result, false);
        }
    }
    #endregion
}
