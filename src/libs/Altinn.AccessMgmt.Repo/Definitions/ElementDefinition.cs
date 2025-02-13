using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Element

public class ElementDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<Element>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.Urn);
            def.RegisterProperty(t => t.TypeId);
            def.RegisterProperty(t => t.ResourceId);

            def.RegisterExtendedProperty<ExtElement, ElementType>(t => t.TypeId, t => t.Id, t => t.Type, cascadeDelete: false);
            def.RegisterExtendedProperty<ExtElement, Resource>(t => t.ResourceId, t => t.Id, t => t.Resource, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.Name]);
        });
    }
}

#endregion
