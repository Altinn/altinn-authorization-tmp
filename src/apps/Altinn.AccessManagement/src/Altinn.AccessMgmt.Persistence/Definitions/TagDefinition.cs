using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class TagDefinition : BaseDbDefinition<Tag>, IDbDefinition
{
    /// <inheritdoc/>
    public TagDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<Tag>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.GroupId!, nullable: true);
            def.RegisterProperty(t => t.ParentId!, nullable: true);

            def.RegisterExtendedProperty<ExtTag, TagGroup>(t => t.GroupId!, t => t.Id, t => t.Group!, optional: true, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtTag, Tag>(t => t.ParentId!, t => t.Id, t => t.Parent!, optional: true, cascadeDelete: true);
            def.RegisterUniqueConstraint([t => t.GroupId!, t => t.Name]);
        });
    }
}
