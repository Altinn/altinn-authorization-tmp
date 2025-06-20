using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;

namespace Altinn.AccessMgmt.PersistenceEF.Services;

public class AreaService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<Area, ExtendedArea, AuditArea>(basicDb, extendedDb, auditDb) { }
public class AreaGroupService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<AreaGroup, ExtendedAreaGroup, AuditAreaGroup>(basicDb, extendedDb, auditDb) { }
public class AssignmentService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<Assignment, ExtendedAssignment, AuditAssignment>(basicDb, extendedDb, auditDb) { }
public class AssignmentPackageService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<AssignmentPackage, ExtendedAssignmentPackage, AuditAssignmentPackage>(basicDb, extendedDb, auditDb) { }
public class AssignmentResourceService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<AssignmentResource, ExtendedAssignmentResource, AuditAssignmentResource>(basicDb, extendedDb, auditDb) { }
public class DelegationService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<Delegation, ExtendedDelegation, AuditDelegation>(basicDb, extendedDb, auditDb) { }
public class DelegationPackageService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<DelegationPackage, ExtendedDelegationPackage, AuditDelegationPackage>(basicDb, extendedDb, auditDb) { }
public class DelegationResourceService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<DelegationResource, ExtendedDelegationResource, AuditDelegationResource>(basicDb, extendedDb, auditDb) { }
public class EntityService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<Entity, ExtendedEntity, AuditEntity>(basicDb, extendedDb, auditDb) { }
public class EntityLookupService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<EntityLookup, ExtendedEntityLookup, AuditEntityLookup>(basicDb, extendedDb, auditDb) { }
public class EntityTypeService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<EntityType, ExtendedEntityType, AuditEntityType>(basicDb, extendedDb, auditDb) { }
public class EntityVariantService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<EntityVariant, ExtendedEntityVariant, AuditEntityVariant>(basicDb, extendedDb, auditDb) { }
public class EntityVariantRoleService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<EntityVariantRole, ExtendedEntityVariantRole, AuditEntityVariantRole>(basicDb, extendedDb, auditDb) { }
public class PackageService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<Package, ExtendedPackage, AuditPackage>(basicDb, extendedDb, auditDb) { }
public class PackageResourceService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<PackageResource, ExtendedPackageResource, AuditPackageResource>(basicDb, extendedDb, auditDb) { }
public class ProviderService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<Provider, ExtendedProvider, AuditProvider>(basicDb, extendedDb, auditDb) { }
public class ProviderTypeService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<ProviderType, ExtendedProviderType, AuditProviderType>(basicDb, extendedDb, auditDb) { }
public class ResourceService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<Resource, ExtendedResource, AuditResource>(basicDb, extendedDb, auditDb) { }
public class ResourceTypeService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<ResourceType, ExtendedResourceType, AuditResourceType>(basicDb, extendedDb, auditDb) { }
public class RoleService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<Role, ExtendedRole, AuditRole>(basicDb, extendedDb, auditDb) { }
public class RoleLookupService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<RoleLookup, ExtendedRoleLookup, AuditRoleLookup>(basicDb, extendedDb, auditDb) { }
public class RoleMapService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<RoleMap, ExtendedRoleMap, AuditRoleMap>(basicDb, extendedDb, auditDb) { }
public class RolePackageService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<RolePackage, ExtendedRolePackage, AuditRolePackage>(basicDb, extendedDb, auditDb) { }
public class RoleResourceService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb) : BaseService<RoleResource, ExtendedRoleResource, AuditRoleResource>(basicDb, extendedDb, auditDb) { }
