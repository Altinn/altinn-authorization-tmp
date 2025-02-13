using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Tag
#endregion

#region Resource
public class ResourceTypeDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<ResourceType>(def =>
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

#endregion
