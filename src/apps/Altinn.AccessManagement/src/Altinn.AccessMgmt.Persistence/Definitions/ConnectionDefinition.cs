﻿using System.Text;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class ConnectionDefinition : BaseDbDefinition<Connection>, IDbDefinition
{
    /// <inheritdoc/>
    public ConnectionDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<Connection>(def =>
        {
            def.IsView();

            def.RegisterProperty(t => t.Id);
            def.RegisterProperty(t => t.FromId);
            def.RegisterProperty(t => t.RoleId);
            def.RegisterProperty(t => t.FacilitatorId, nullable: true);
            def.RegisterProperty(t => t.ToId);
            def.RegisterProperty(t => t.FacilitatorRoleId, nullable: true);

            def.RegisterExtendedProperty<ExtConnection, Entity>(t => t.FromId, t => t.Id, t => t.From);
            def.RegisterExtendedProperty<ExtConnection, Role>(t => t.RoleId, t => t.Id, t => t.FromRole);
            def.RegisterExtendedProperty<ExtConnection, Entity>(t => t.FacilitatorId, t => t.Id, t => t.Facilitator, optional: true);
            def.RegisterExtendedProperty<ExtConnection, Entity>(t => t.ToId, t => t.Id, t => t.To);
            def.RegisterExtendedProperty<ExtConnection, Role>(t => t.FacilitatorRoleId, t => t.Id, t => t.ToRole, optional: true);

            var sb = new StringBuilder();

            // Basic Assignment
            sb.AppendLine($"SELECT a.{nameof(Assignment.Id)}, a.{nameof(Assignment.FromId)} AS {nameof(Connection.FromId)}, NULL AS {nameof(Connection.FacilitatorId)}, a.{nameof(Connection.ToId)}, a.{nameof(Assignment.RoleId)} AS {nameof(Connection.RoleId)}, NULL AS {nameof(Connection.FacilitatorRoleId)}");
            sb.AppendLine("FROM dbo.assignment AS a");
            sb.AppendLine("UNION ALL");

            // Inheireted Roles from RoleMap
            sb.AppendLine($"SELECT a.{nameof(Assignment.Id)}, a.{nameof(Assignment.FromId)}, NULL AS {nameof(Connection.FacilitatorId)}, a.{nameof(Connection.ToId)}, rm.{nameof(RoleMap.GetRoleId)} AS {nameof(Connection.FacilitatorRoleId)}, NULL AS {nameof(Connection.FacilitatorRoleId)}");
            sb.AppendLine("FROM dbo.assignment AS a");
            sb.AppendLine($"INNER JOIN dbo.rolemap as rm on a.{nameof(Assignment.RoleId)} = rm.{nameof(RoleMap.HasRoleId)}");
            sb.AppendLine("UNION ALL");

            // Delegations
            sb.AppendLine($"SELECT d.{nameof(Delegation.Id)}, fa.{nameof(Assignment.FromId)} AS {nameof(Connection.FromId)}, fa.{nameof(Assignment.ToId)} AS {nameof(Connection.FacilitatorId)}, ta.{nameof(Assignment.ToId)} AS {nameof(Connection.ToId)}, fa.{nameof(Assignment.RoleId)} AS {nameof(Connection.RoleId)}, ta.{nameof(Assignment.RoleId)} AS {nameof(Connection.FacilitatorRoleId)}");
            sb.AppendLine("FROM dbo.delegation AS d");
            sb.AppendLine($"INNER JOIN dbo.assignment AS fa ON d.{nameof(Delegation.FromId)} = fa.{nameof(Assignment.Id)}");
            sb.AppendLine($"INNER JOIN dbo.assignment AS ta ON d.{nameof(Delegation.ToId)} = ta.{nameof(Assignment.Id)};");

            def.SetViewQuery(sb.ToString());

            def.AddViewDependency<Assignment>();
            def.AddViewDependency<Delegation>();
            def.AddViewDependency<RoleMap>();
        });
    }
}
