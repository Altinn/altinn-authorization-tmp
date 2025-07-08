using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class RoleDefinition : BaseDbDefinition<Role>, IDbDefinition
{
    /// <inheritdoc/>
    public RoleDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<Role>(def =>
        {
            def.EnableAudit();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.Code);
            def.RegisterProperty(t => t.Urn);
            def.RegisterProperty(t => t.Description);
            def.RegisterProperty(t => t.IsKeyRole, defaultValue: "false");
            def.RegisterProperty(t => t.IsAssignable, defaultValue: "false");
            def.RegisterProperty(t => t.ProviderId);
            def.RegisterProperty(t => t.EntityTypeId, nullable: true);

            def.RegisterExtendedProperty<ExtRole, Provider>(t => t.ProviderId, t => t.Id, t => t.Provider, cascadeDelete: false);
            def.RegisterExtendedProperty<ExtRole, EntityType>(t => t.EntityTypeId, t => t.Id, t => t.EntityType, cascadeDelete: false, optional: true);

            def.RegisterUniqueConstraint([t => t.Urn]);
            def.RegisterUniqueConstraint([t => t.ProviderId, t => t.Name]);
            def.RegisterUniqueConstraint([t => t.ProviderId, t => t.Code]);

            def.AddManualPreMigrationScript(1, GetPreMigrationScript_EntityTypeIdUniqueConstraint());
            def.AddManualPreMigrationScript(2, GetPreMigrationScript_EntityTypeIdNullable());
            def.AddManualPreMigrationScript(3, GetPreMigrationScript_DeleteOldAdminRoles());
        });
    }

    private string GetPreMigrationScript_EntityTypeIdUniqueConstraint()
    {
        return """
            DO
            $$
            BEGIN
              IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'dbo' AND table_name = 'role') THEN
                ALTER TABLE dbo.role DROP CONSTRAINT IF EXISTS uc_role_entitytypeid_code;
                ALTER TABLE dbo.role DROP CONSTRAINT IF EXISTS uc_role_entitytypeid_name;
                DROP INDEX IF EXISTS dbo.uc_role_entitytypeid_code_idx;
                DROP INDEX IF EXISTS dbo.uc_role_entitytypeid_name_idx;
              END IF;
            END
            $$;
            """;
    }

    private string GetPreMigrationScript_EntityTypeIdNullable()
    {
        return """
            DO
            $$
            BEGIN
              IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'dbo' AND table_name = 'role' AND column_name = 'entitytypeid' AND is_nullable = 'NO') THEN
                ALTER TABLE dbo.role ALTER COLUMN entitytypeid DROP NOT NULL;
              END IF;
            END
            $$;
            """;
    }

    private string GetPreMigrationScript_DeleteOldAdminRoles()
    {
        return """
            DO
            $$
            BEGIN
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'dbo' AND table_name = 'role') THEN
                
                    CREATE TEMP TABLE session_audit_context ON COMMIT DROP AS
                    SELECT 
                      '3296007F-F9EA-4BD0-B6A6-C8462D54633A'::uuid AS changed_by,
                      '3296007F-F9EA-4BD0-B6A6-C8462D54633A'::uuid AS changed_by_system,
                      '3296007F-F9EA-4BD0-B6A6-C8462D54633A'::text AS change_operation_id;

                    DELETE FROM dbo.role
                    WHERE urn IN ('urn:altinn:role:klientadministrator', 'urn:altinn:role:tilgangsstyrer', 'urn:altinn:role:maskinporten-administrator');

                END IF;
            END
            $$;
            """;
    }
}
