using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Definitions;
using Altinn.AccessMgmt.Persistence.Core.Executors;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Services;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.AccessMgmt.Persistence.Repositories;

/// <summary>
/// Data service for PackageTag
/// </summary>
public class PackageTagRepository : CrossRepository<PackageTag, ExtPackageTag, Package, Tag>, IPackageTagRepository
{
    /// <inheritdoc/>
    public PackageTagRepository(IOptions<DbAccessConfig> options, DbDefinitionRegistry dbDefinitionRegistry, IDbExecutor executor) : base(options, dbDefinitionRegistry, executor)
    {
    }
}
