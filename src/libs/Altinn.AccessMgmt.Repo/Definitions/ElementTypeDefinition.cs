using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Element
public class ElementTypeDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<ElementType>(def =>
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
