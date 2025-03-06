using System.Text;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class ConnectionPackageDefinition : BaseDbDefinition<ConnectionPackage>, IDbDefinition
{
    /// <inheritdoc/>
    public ConnectionPackageDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<ConnectionPackage>(def =>
        {
            def.IsView();

            def.RegisterProperty(t => t.Id);
            def.RegisterProperty(t => t.ConnectionId);
            def.RegisterProperty(t => t.PackageId);

            def.RegisterAsCrossReferenceExtended<ExtConnectionPackage, Connection, Package>(
                defineA: (t => t.ConnectionId, t => t.Id, t => t.Connection, false),
                defineB: (t => t.PackageId, t => t.Id, t => t.Package, false)
            );

            var sb = new StringBuilder();
            
            sb.AppendLine($"SELECT ap.{nameof(AssignmentPackage.AssignmentId)} AS {nameof(ConnectionPackage.ConnectionId)}, ap.{nameof(AssignmentPackage.PackageId)} AS {nameof(ConnectionPackage.PackageId)}");
            sb.AppendLine("FROM dbo.assignmentpackage AS ap");
            sb.AppendLine("UNION ALL");
            sb.AppendLine($"SELECT dp.{nameof(DelegationPackage.DelegationId)} AS {nameof(ConnectionPackage.ConnectionId)}, dp.{nameof(DelegationPackage.PackageId)} AS {nameof(ConnectionPackage.PackageId)}");
            sb.AppendLine("FROM dbo.delegationpackage AS dp");

            def.SetViewQuery(sb.ToString());

            def.AddViewDependency<Connection>();
            def.AddViewDependency<Package>();
        });
    }
}


/// <inheritdoc/>
public class ConnectionResourceDefinition : BaseDbDefinition<ConnectionResource>, IDbDefinition
{
    /// <inheritdoc/>
    public ConnectionResourceDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<ConnectionResource>(def =>
        {
            def.IsView();

            def.RegisterProperty(t => t.Id);
            def.RegisterProperty(t => t.ConnectionId);
            def.RegisterProperty(t => t.ResourceId);

            def.RegisterAsCrossReferenceExtended<ExtConnectionResource, Connection, Resource>(
                defineA: (t => t.ConnectionId, t => t.Id, t => t.Connection, false),
                defineB: (t => t.ResourceId, t => t.Id, t => t.Resource, false)
            );

            var sb = new StringBuilder();

            sb.AppendLine($"SELECT ap.{nameof(AssignmentResource.AssignmentId)} AS {nameof(ConnectionResource.ConnectionId)}, ap.{nameof(AssignmentResource.ResourceId)} AS {nameof(ConnectionResource.ResourceId)}");
            sb.AppendLine("FROM dbo.assignmentResource AS ap");
            sb.AppendLine("UNION ALL");
            sb.AppendLine($"SELECT dp.{nameof(DelegationResource.DelegationId)} AS {nameof(ConnectionResource.ConnectionId)}, dp.{nameof(DelegationResource.ResourceId)} AS {nameof(ConnectionResource.ResourceId)}");
            sb.AppendLine("FROM dbo.delegationResource AS dp");

            def.SetViewQuery(sb.ToString());

            def.AddViewDependency<Connection>();
            def.AddViewDependency<Resource>();
        });
    }
}
