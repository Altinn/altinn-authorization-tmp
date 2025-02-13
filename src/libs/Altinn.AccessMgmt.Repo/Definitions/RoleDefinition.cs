using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Role

public class RoleDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<Role>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.Code);
            def.RegisterProperty(t => t.Urn);
            def.RegisterProperty(t => t.Description);
            def.RegisterProperty(t => t.ProviderId);
            def.RegisterProperty(t => t.EntityTypeId);

            def.RegisterExtendedProperty<ExtRole, Provider>(t => t.ProviderId, t => t.Id, t => t.Provider, cascadeDelete: false);
            def.RegisterExtendedProperty<ExtRole, EntityType>(t => t.EntityTypeId, t => t.Id, t => t.EntityType, cascadeDelete: false);

            def.RegisterUniqueConstraint([t => t.Urn]);
            def.RegisterUniqueConstraint([t => t.EntityTypeId, t => t.Name]);
            def.RegisterUniqueConstraint([t => t.EntityTypeId, t => t.Code]);
        });
    }
}

#endregion
