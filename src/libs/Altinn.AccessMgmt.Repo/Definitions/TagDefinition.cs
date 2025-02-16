using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class TagDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<Tag>(def =>
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
            def.RegisterUniqueConstraint([t => t.GroupId, t => t.Name]);
        });
    }
}
