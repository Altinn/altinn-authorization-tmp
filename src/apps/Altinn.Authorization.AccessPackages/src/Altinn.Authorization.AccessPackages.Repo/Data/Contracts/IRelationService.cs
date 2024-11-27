using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

//// TODO: IVAR

public interface IRelationService : IDbExtendedDataService<Relation, ExtRelation> { }

public interface IRelationAssignmentService : IDbExtendedDataService<RelationAssignment, ExtRelationAssignment> { }
