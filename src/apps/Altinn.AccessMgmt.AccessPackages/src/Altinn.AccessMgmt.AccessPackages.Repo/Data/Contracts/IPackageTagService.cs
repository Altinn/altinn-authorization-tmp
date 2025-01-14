using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;

/// <inheritdoc/>
public interface IPackageTagService : IDbCrossDataService<Package, PackageTag, Tag> { }
