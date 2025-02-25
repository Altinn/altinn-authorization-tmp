using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class WorkerConfigDefinition : BaseDbDefinition<WorkerConfig>, IDbDefinition
{
    /// <inheritdoc/>
    public WorkerConfigDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<WorkerConfig>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Key);
            def.RegisterProperty(t => t.Value);

            def.RegisterUniqueConstraint([t => t.Key]);
        });
    }
}
