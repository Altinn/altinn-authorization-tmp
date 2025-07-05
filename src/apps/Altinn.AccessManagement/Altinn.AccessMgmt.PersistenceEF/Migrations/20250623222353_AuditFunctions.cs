using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class AuditFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION dbo.audit_insert_fn()
                RETURNS TRIGGER AS $$
                DECLARE
                changed_by UUID;
                changed_by_system UUID;
                change_operation_id text;
                BEGIN
                SELECT current_setting('app.changed_by', false) INTO changed_by;
                SELECT current_setting('app.changed_by_system', false) INTO changed_by_system;
                SELECT current_setting('app.change_operation_id', false) INTO change_operation_id;
                NEW.audit_changedby := changed_by;
                NEW.audit_changedbysystem := changed_by_system;
                NEW.audit_changeoperation := change_operation_id;
                NEW.audit_validfrom := now();
                RETURN NEW;
                END;
                $$;
               """);

            // string columns = string.Join(',', _definition.Properties.Select(t => t.Name));
            // string oldColumns = string.Join(',', _definition.Properties.Select(t => $"OLD.{t.Name}"));

            // UPDATE FUNCTION
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION {schema}.audit_{modelName}_update_fn()
                RETURNS TRIGGER AS $$
                BEGIN
                    INSERT INTO {historyTableName} (
                    {columns},
                    audit_validfrom, audit_validto,
                    audit_changedby, audit_changedbysystem, audit_changeoperation
                    ) VALUES (
                    {oldColumns},
                    OLD.audit_validfrom, now(),
                    OLD.audit_changedby, OLD.audit_changedbysystem, OLD.audit_changeoperation
                    );
                    RETURN NEW;
                END;
                $$;
                """);

            // DELETE FUNCTION
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION {schema}.audit_{modelName}_delete_fn()
                RETURNS TRIGGER AS $$
                DECLARE ctx RECORD;
                BEGIN
                    SELECT * INTO ctx FROM session_audit_context LIMIT 1;
                    INSERT INTO {historyTableName} (
                    {columns},
                    audit_validfrom, audit_validto,
                    audit_changedby, audit_changedbysystem, audit_changeoperation,
                    audit_deletedby, audit_deletedbysystem, audit_deleteoperation
                    ) VALUES (
                    {oldColumns},
                    OLD.audit_validfrom, now(),
                    OLD.audit_changedby, OLD.audit_changedbysystem, OLD.audit_changeoperation,
                    ctx.changed_by, ctx.changed_by_system, ctx.change_operation_id
                    );
                    RETURN OLD;
                END;
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
        }
    }
}
