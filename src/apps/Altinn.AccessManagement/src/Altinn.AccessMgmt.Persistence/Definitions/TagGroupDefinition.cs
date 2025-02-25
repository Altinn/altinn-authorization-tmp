using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class TagGroupDefinition : BaseDbDefinition<TagGroup>, IDbDefinition
{
    /// <inheritdoc/>
    public TagGroupDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<TagGroup>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);
            def.RegisterProperty(t => t.Name);
            def.RegisterUniqueConstraint([t => t.Name]);
        });
    }
}
