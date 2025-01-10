using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;

/// <inheritdoc/>
public interface IResourceGroupService : IDbExtendedDataService<ResourceGroup, ExtResourceGroup> { }