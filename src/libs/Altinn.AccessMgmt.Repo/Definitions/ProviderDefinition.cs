using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;

public class ProviderDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<Provider>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);
            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.RefId, nullable: true);
            def.RegisterUniqueConstraint([t => t.Name]);
        });
    }
}

#endregion
