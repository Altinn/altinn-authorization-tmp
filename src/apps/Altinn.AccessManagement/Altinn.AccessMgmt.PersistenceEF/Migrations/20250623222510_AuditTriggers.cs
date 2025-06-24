using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class AuditTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // INSERT TRIGGER
            migrationBuilder.Sql("CREATE OR REPLACE TRIGGER {triggerName} BEFORE INSERT OR UPDATE ON {tableName} FOR EACH ROW EXECUTE FUNCTION dbo.audit_insert_fn();");

            // UPDATE TRIGGER
            migrationBuilder.Sql("CREATE OR REPLACE TRIGGER {triggerName} AFTER UPDATE ON {tableName} FOR EACH ROW EXECUTE FUNCTION {schema}.{functionName}();");

            // DELETE TRIGGER
            migrationBuilder.Sql("CREATE OR REPLACE TRIGGER {triggerName} AFTER DELETE ON {tableName} FOR EACH ROW EXECUTE FUNCTION {schema}.{functionName}();");


            // AreaGroup
            migrationBuilder.Sql("CREATE OR REPLACE TRIGGER Trigger_Audit_AreaGroup_Insert BEFORE INSERT OR UPDATE ON dbo.areagroup FOR EACH ROW EXECUTE FUNCTION dbo.audit_insert_fn();");
            migrationBuilder.Sql("CREATE OR REPLACE TRIGGER Trigger_Audit_AreaGroup_Update AFTER UPDATE ON dbo.areagroup FOR EACH ROW EXECUTE FUNCTION dbo.audit_areagroup_update_fn();");
            migrationBuilder.Sql("CREATE OR REPLACE TRIGGER Trigger_Audit_AreaGroup_Delete AFTER DELETE ON dbo.areagroup FOR EACH ROW EXECUTE FUNCTION dbo.audit_areagroup_delete_fn();");

            // Area
            migrationBuilder.Sql("CREATE OR REPLACE TRIGGER Trigger_Audit_Area_Insert BEFORE INSERT OR UPDATE ON dbo.area FOR EACH ROW EXECUTE FUNCTION dbo.audit_insert_fn();");
            migrationBuilder.Sql("CREATE OR REPLACE TRIGGER Trigger_Audit_Area_Update AFTER UPDATE ON dbo.area FOR EACH ROW EXECUTE FUNCTION dbo.audit_area_update_fn();");
            migrationBuilder.Sql("CREATE OR REPLACE TRIGGER Trigger_Audit_Area_Delete AFTER DELETE ON dbo.area FOR EACH ROW EXECUTE FUNCTION dbo.audit_area_delete_fn();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
