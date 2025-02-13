using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Area
public class AreaGroupDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<AreaGroup>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.Description);
            def.RegisterProperty(t => t.EntityTypeId, nullable: true);
            def.RegisterProperty(t => t.Urn, nullable: true);

            def.RegisterExtendedProperty<ExtAreaGroup, EntityType>(t => t.EntityTypeId, t => t.Id, t => t.EntityType, cascadeDelete: true);
            def.RegisterUniqueConstraint([t => t.Name]);
        });
    }
}

#endregion
