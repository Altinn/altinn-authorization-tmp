using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class GroupMemberDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<GroupMember>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.GroupId);
            def.RegisterProperty(t => t.MemberId);
            def.RegisterProperty(t => t.ActiveFrom, nullable: true);
            def.RegisterProperty(t => t.ActiveTo, nullable: true);

            def.RegisterExtendedProperty<ExtGroupMember, EntityGroup>(t => t.GroupId, t => t.Id, t => t.Group, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtGroupMember, Entity>(t => t.MemberId, t => t.Id, t => t.Member, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.GroupId, t => t.MemberId]);
        });
    }
}
