using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class PackageTagDefinition : BaseDbDefinition<PackageTag>, IDbDefinition
{
    /// <inheritdoc/>
    public PackageTagDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<PackageTag>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.PackageId);
            def.RegisterProperty(t => t.TagId);

            def.RegisterAsCrossReferenceExtended<ExtPackageTag, Package, Tag>(
               defineA: (t => t.PackageId, t => t.Id, t => t.Package, CascadeDelete: true),
               defineB: (t => t.TagId, t => t.Id, t => t.Tag, CascadeDelete: false)
            );

            def.RegisterUniqueConstraint([t => t.PackageId, t => t.TagId]);
        });
    }
}
