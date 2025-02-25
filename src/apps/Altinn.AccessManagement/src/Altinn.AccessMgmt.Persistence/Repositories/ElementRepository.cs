using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Definitions;
using Altinn.AccessMgmt.Persistence.Core.Executors;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Services;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.AccessMgmt.Persistence.Repositories;

/// <summary>
/// Data service for Element
/// </summary>
public class ElementRepository : ExtendedRepository<Element, ExtElement>, IElementRepository
{
    /// <inheritdoc/>
    public ElementRepository(IOptions<DbAccessConfig> options, DbDefinitionRegistry dbDefinitionRegistry, IDbExecutor executor) : base(options, dbDefinitionRegistry, executor)
    {
    }
}
