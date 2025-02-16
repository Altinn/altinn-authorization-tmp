using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class AssignmentResourceDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<AssignmentResource>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.AssignmentId);
            def.RegisterProperty(t => t.ResourceId);

            def.RegisterExtendedProperty<ExtAssignmentResource, Entity>(t => t.AssignmentId, t => t.Id, t => t.Assignment, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtAssignmentResource, Entity>(t => t.ResourceId, t => t.Id, t => t.Resource, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.AssignmentId, t => t.ResourceId]);
        });
    }
}
