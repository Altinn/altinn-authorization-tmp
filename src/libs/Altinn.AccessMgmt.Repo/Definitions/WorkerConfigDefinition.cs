using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class WorkerConfigDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<WorkerConfig>(def =>
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
