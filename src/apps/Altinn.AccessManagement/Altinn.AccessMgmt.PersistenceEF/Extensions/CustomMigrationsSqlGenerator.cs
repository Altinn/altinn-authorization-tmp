using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions;

public class CustomMigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
{
    public CustomMigrationsSqlGenerator(
        MigrationsSqlGeneratorDependencies dependencies,
        IRelationalAnnotationProvider annotations
    ) : base(dependencies, (Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal.INpgsqlSingletonOptions)annotations) { }

    protected override void Generate(CreateTableOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
    {
        base.Generate(operation, model, builder, terminate);

        var entityType = model?.GetEntityTypes().FirstOrDefault(et =>
            et.GetTableName() == operation.Name &&
            et.GetSchema() == operation.Schema);

        var enableAudit = model.FindAnnotation("EnableAudit"); // Easy?

        if (entityType?.FindAnnotation("EnableAudit")?.Value as bool? == true)
        {
            builder.AppendLine(GenerateAuditInsertFunctionAndTrigger(operation.Schema, operation.Name, new List<string>()));
            builder.AppendLine(GenerateAuditUpdateFunctionAndTrigger(operation.Schema, operation.Name, new List<string>()));
            builder.AppendLine(GenerateAuditDeleteFunctionAndTrigger(operation.Schema, operation.Name, new List<string>()));
        }

        if (entityType?.FindAnnotation("EnableTranslation")?.Value as bool? == true)
        {
            // Find all properties with annotation "Translate"
            // Moved to TranslationService
        }
    }

    private string GenerateAuditInsertFunctionAndTrigger(string schema, string name, List<string> columns)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"CREATE OR REPLACE FUNCTION {schema}.set_audit_generic_insert_fn()");
        sb.AppendLine("RETURNS TRIGGER AS $$");
        sb.AppendLine("DECLARE ctx RECORD;");
        sb.AppendLine("BEGIN");
        sb.AppendLine("SELECT * INTO ctx FROM session_audit_context LIMIT 1;");
        sb.AppendLine("NEW.audit_changedby := ctx.changed_by;");
        sb.AppendLine("NEW.audit_changedbysystem := ctx.changed_by_system;");
        sb.AppendLine("NEW.audit_changeoperation := ctx.change_operation_id;");
        sb.AppendLine("NEW.audit_validfrom := now();");
        sb.AppendLine("RETURN NEW;");
        sb.AppendLine("END;");
        sb.AppendLine("$$ LANGUAGE plpgsql;");

        sb.AppendLine($"DO $$ BEGIN IF NOT EXISTS (SELECT * FROM pg_trigger t WHERE t.tgname ILIKE 'audit_{name}_insert_trg' AND t.tgrelid = '{name}'::regclass) THEN");
        sb.AppendLine($"CREATE OR REPLACE TRIGGER audit_{name}_insert_trg BEFORE INSERT OR UPDATE ON {schema}.{name}");
        sb.AppendLine($"FOR EACH ROW EXECUTE FUNCTION {schema}.audit_generic_insert_fn();");
        sb.AppendLine($"END IF; END $$;");

        return sb.ToString();
    }

    private string GenerateAuditUpdateFunctionAndTrigger(string schema, string name, List<string> columns)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"CREATE OR REPLACE FUNCTION {schema}.audit_{name}_update_fn()");
        sb.AppendLine("RETURNS TRIGGER AS $$");
        sb.AppendLine("BEGIN");
        sb.AppendLine($"INSERT INTO audit._{name} (");
        sb.AppendLine($"{string.Join(',', columns)},");
        sb.AppendLine("audit_validfrom, audit_validto,");
        sb.AppendLine("audit_changedby, audit_changedbysystem, audit_changeoperation");
        sb.AppendLine(") VALUES (");
        sb.AppendLine($"{string.Join(',', columns.Select(t => "OLD." + t))},");
        sb.AppendLine("OLD.audit_validfrom, now(),");
        sb.AppendLine("OLD.audit_changedby, OLD.audit_changedbysystem, OLD.audit_changeoperation");
        sb.AppendLine(");");
        sb.AppendLine("RETURN NEW;");
        sb.AppendLine("END;");
        sb.AppendLine("$$ LANGUAGE plpgsql;");

        sb.AppendLine($"DO $$ BEGIN IF NOT EXISTS (SELECT * FROM pg_trigger t WHERE t.tgname ILIKE 'audit_{name}_update_trg' AND t.tgrelid = '{name}'::regclass) THEN");
        sb.AppendLine($"CREATE OR REPLACE TRIGGER audit_{name}_update_trg AFTER UPDATE ON {schema}.{name}");
        sb.AppendLine($"FOR EACH ROW EXECUTE FUNCTION {schema}.audit_{name}_update_fn();");
        sb.AppendLine($"END IF; END $$;");

        return sb.ToString();
    }

    private string GenerateAuditDeleteFunctionAndTrigger(string schema, string name, List<string> columns)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"CREATE OR REPLACE FUNCTION {schema}.audit_{name}_delete_fn()");
        sb.AppendLine("RETURNS TRIGGER AS $$");
        sb.AppendLine("DECLARE ctx RECORD;");
        sb.AppendLine("BEGIN");
        sb.AppendLine("SELECT * INTO ctx FROM session_audit_context LIMIT 1;");
        sb.AppendLine($"INSERT INTO audit._{name} (");
        sb.AppendLine($"{string.Join(',', columns)},");
        sb.AppendLine("audit_validfrom, audit_validto,");
        sb.AppendLine("audit_changedby, audit_changedbysystem, audit_changeoperation,");
        sb.AppendLine("audit_deletedby, audit_deletedbysystem, audit_deleteoperation");
        sb.AppendLine(") VALUES (");
        sb.AppendLine($"{string.Join(',', columns.Select(t => "OLD." + t))},");
        sb.AppendLine("OLD.audit_validfrom, now(),");
        sb.AppendLine("OLD.audit_changedby, OLD.audit_changedbysystem, OLD.audit_changeoperation,");
        sb.AppendLine("ctx.changed_by, ctx.changed_by_system, ctx.change_operation_id");
        sb.AppendLine(");");
        sb.AppendLine("RETURN OLD;");
        sb.AppendLine("END;");
        sb.AppendLine("$$ LANGUAGE plpgsql;");

        sb.AppendLine($"DO $$ BEGIN IF NOT EXISTS (SELECT * FROM pg_trigger t WHERE t.tgname ILIKE 'audit_{name}_delete_trg' AND t.tgrelid = '{name}'::regclass) THEN");
        sb.AppendLine($"CREATE OR REPLACE TRIGGER audit_{name}_delete_trg AFTER DELETE ON {schema}.{name}");
        sb.AppendLine($"FOR EACH ROW EXECUTE FUNCTION {schema}.audit_{name}_delete_fn();");
        sb.AppendLine($"END IF; END $$;");

        return sb.ToString();
    }
}
