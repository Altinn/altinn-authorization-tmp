using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class DelegationDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<Delegation>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.FromId);
            def.RegisterProperty(t => t.ToId);
            def.RegisterProperty(t => t.SourceId);
            def.RegisterProperty(t => t.ViaId);

            def.RegisterExtendedProperty<ExtDelegation, Assignment>(t => t.FromId, t => t.Id, t => t.From, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtDelegation, Assignment>(t => t.ToId, t => t.Id, t => t.To, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtDelegation, Entity>(t => t.SourceId, t => t.Id, t => t.Source, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtDelegation, Entity>(t => t.ViaId, t => t.Id, t => t.Via, cascadeDelete: true);

            //def.RegisterUniqueConstraint([t => t.FromId, t => t.ToId]);
        });
    }
}
