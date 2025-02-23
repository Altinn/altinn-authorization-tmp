using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Contracts;

/// <inheritdoc/>
public interface IPackageTagService : IDbCrossRepository<PackageTag, ExtPackageTag, Package, Tag> { }
