using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions;

public static class ModelBuilderExtensions
{
    public static void UseLowerCaseNamingConvention(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Lowercase tabellnavn
            entity.SetTableName(entity.GetTableName()?.ToLowerInvariant());
            entity.SetSchema(entity.GetSchema()?.ToLowerInvariant());

            // Lowercase kolonnenavn
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName(StoreObjectIdentifier.Table(entity.GetTableName(), entity.GetSchema()))?.ToLowerInvariant());
            }

            // Lowercase primary keys
            foreach (var key in entity.GetKeys())
            {
                key.SetName(key.GetName()?.ToLowerInvariant());
            }

            // Lowercase foreign keys
            foreach (var fk in entity.GetForeignKeys())
            {
                fk.SetConstraintName(fk.GetConstraintName()?.ToLowerInvariant());
            }

            // Lowercase index-navn
            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName()?.ToLowerInvariant());
            }
        }
    }
}
