﻿using System.Data;
using System.Reflection;
using System.Text;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using NpgsqlTypes;

namespace Altinn.AccessMgmt.PersistenceEF.Utils;

public class NewIngestService(IDbConnection dbConnection, AppDbContext dbContext) : INewIngestService
{
    public NpgsqlConnection DbConnection { get; set; } = (NpgsqlConnection)dbConnection;

    public async Task<int> IngestData<T>(List<T> data, CancellationToken cancellationToken = default)
    {
        var model = dbContext.Model;
        if (model == null)
        {
            throw new ArgumentNullException(nameof(T));
        }

        var tableName = GetTableName<T>(model);
        var ingestColumns = GetColumns<T>(model);

        return await WriteToIngest(data, ingestColumns, tableName.TableName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> IngestTempData<T>(List<T> data, Guid ingestId, CancellationToken cancellationToken = default)
    {
        if (ingestId.Equals(Guid.Empty))
        {
            throw new Exception(string.Format("Ingest id '{0}' not valid", ingestId.ToString()));
        }

        var table = GetTableName<T>(dbContext.Model);
        var ingestColumns = GetColumns<T>(dbContext.Model);

        string columnStatement = string.Join(',', ingestColumns.Select(t => t.Name));

        var ingestName = ingestId.ToString().Replace("-", string.Empty);
        string ingestTableName = "ingest." + table.TableName + "_" + ingestName;

        var createIngestTable = $"CREATE UNLOGGED TABLE IF NOT EXISTS {ingestTableName} AS SELECT {columnStatement} FROM {table.SchemaName}.{table.TableName} WITH NO DATA;";

        await dbContext.Database.ExecuteSqlRawAsync(createIngestTable, cancellationToken);

        var completed = await WriteToIngest(data, ingestColumns, ingestTableName, cancellationToken);

        return completed;
    }

    /// <inheritdoc />
    public async Task<int> MergeTempData<T>(Guid ingestId, AuditValues auditValues, IEnumerable<string> matchColumns = null, CancellationToken cancellationToken = default)
    {
        if (matchColumns == null || matchColumns.Count() == 0)
        {
            matchColumns = ["id"];
        }

        var table = GetTableName<T>(dbContext.Model);
        var ingestColumns = GetColumns<T>(dbContext.Model);

        string columnStatement = string.Join(',', ingestColumns.Select(t => t.Name));

        var ingestName = ingestId.ToString().Replace("-", string.Empty);
        string ingestTableName = "ingest." + table.TableName + "_" + ingestName;

        var mergeMatchStatement = string.Join(" AND ", matchColumns.Select(t => $"(target.{t} IS NULL AND source.{t} IS NULL OR target.{t} = source.{t})"));
        var mergeUpdateUnMatchStatement = string.Join(
            " OR ",
            ingestColumns
                .Where(t => matchColumns.Count(y => y.Equals(t.Name, StringComparison.OrdinalIgnoreCase)) == 0)
                .Select(t =>
                    $"(" +
                    $"target.{t.Name} <> source.{t.Name} " +
                    $"OR (target.{t.Name} IS NULL AND source.{t.Name} IS NOT NULL) " +
                    $"OR (target.{t.Name} IS NOT NULL AND source.{t.Name} IS NULL)" +
                    $")"
                )
        );

        string mergeUpdateStatement = string.Join(", ", ingestColumns.Where(t => !matchColumns.Any(y => y.Equals(t.Name, StringComparison.OrdinalIgnoreCase))).Select(t => $"{t.Name} = source.{t.Name}"));

        var insertColumns = string.Join(", ", ingestColumns.Select(t => $"{t.Name}"));
        var insertValues = string.Join(", ", ingestColumns.Select(t => $"source.{t.Name}"));

        var sb = new StringBuilder();

        sb.AppendLine(GetAuditVariables(auditValues));
        sb.AppendLine($"MERGE INTO {table.SchemaName}.{table.TableName} AS target USING {ingestTableName} AS source ON {mergeMatchStatement}");
        sb.AppendLine($"WHEN NOT MATCHED THEN ");
        sb.AppendLine($"INSERT ({insertColumns}) VALUES ({insertValues});");

        string mergeStatement = sb.ToString();

        Console.WriteLine("Starting MERGE");

        var res = await ExecuteMigrationCommand(mergeStatement, cancellationToken: cancellationToken);

        Console.WriteLine("Cleanup");
        var dropIngestTable = $"DROP TABLE IF EXISTS {ingestTableName};";
        await ExecuteMigrationCommand(dropIngestTable, cancellationToken: cancellationToken);

        Console.WriteLine($"Merged {res}");

        return res;
    }

    /// <inheritdoc />
    public async Task<int> IngestAndMergeData<T>(List<T> data, AuditValues auditValues, IEnumerable<string> matchColumns = null, CancellationToken cancellationToken = default)
    {
        var ingestId = Guid.CreateVersion7();
        await IngestTempData(data, ingestId, cancellationToken);
        var res = await MergeTempData<T>(ingestId, auditValues, matchColumns, cancellationToken);

        return res;
    }

    private async Task<int> WriteToIngest<T>(List<T> data, List<IngestColumnDefinition> ingestColumns, string tableName, CancellationToken cancellationToken = default)
    {
        using var conn = new NpgsqlConnection(DbConnection.ConnectionString);
        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }

        string columnStatement = string.Join(',', ingestColumns.Select(t => t.Name));
        using var writer = await conn.BeginBinaryImportAsync($"COPY {tableName} ({columnStatement}) FROM STDIN (FORMAT BINARY)", cancellationToken: cancellationToken);
        writer.Timeout = TimeSpan.FromMinutes(10);
        int completed = 0;
        foreach (var d in data)
        {
            writer.StartRow();
            foreach (var c in ingestColumns)
            {
                try
                {
                    writer.Write(c.Property.GetValue(d), c.Type);
                }
                catch (Exception ex)
                {
                    // Replace with proper logging.
                    Console.WriteLine($"Failed to write data in column '{c.Name}' for '{tableName}'. Trying to write null. " + ex.Message);
                    try
                    {
                        writer.WriteNull();
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to write null in column '{c.Name}' for '{tableName}'.");
                        throw;
                    }
                }
            }

            completed++;
        }

        writer.Complete();

        Console.WriteLine($"Ingested {completed}");
        return completed;
    }

    private async Task<int> ExecuteMigrationCommand(string query, CancellationToken cancellationToken = default)
    {
        return await dbContext.Database.ExecuteSqlRawAsync(query, cancellationToken);
    }

    private Dictionary<Type, List<IngestColumnDefinition>> TypedIngestColumnDefinitions { get; set; } = new Dictionary<Type, List<IngestColumnDefinition>>();

    private List<IngestColumnDefinition> GetColumns<T>(IModel entityModel)
    {
        var typeName = typeof(T).Name;
        var et = entityModel.GetEntityTypes().FirstOrDefault(x => x.GetTableName() == typeName && x.GetSchema() == BaseConfiguration.BaseSchema);
        var storeObject = StoreObjectIdentifier.Table(typeName, BaseConfiguration.BaseSchema);

        return et.GetProperties()
            .Where(n => !n.Name.StartsWith("audit_", StringComparison.OrdinalIgnoreCase))
            .Select(p => new IngestColumnDefinition() { Name = p.GetColumnName(storeObject), Property = p.PropertyInfo, DbTypeName = p.GetColumnType() })
            .Distinct()
            .ToList()!;
    }

    private (string TableName, string SchemaName) GetTableName<T>(IModel entityModel)
    {
        return (typeof(T).Name.ToLower(), BaseConfiguration.BaseSchema);
    }
    
    private static string GetAuditVariables(AuditValues auditValues)
    {
        return string.Format("SET LOCAL app.changed_by = '{0}'; SET LOCAL app.changed_by_system = '{1}'; SET LOCAL app.change_operation_id = '{2}';", auditValues.ChangedBy, auditValues.ChangedBySystem, auditValues.OperationId);
    }
}

/// <summary>
/// Ingest data service
/// </summary>
public interface INewIngestService
{
    /// <summary>
    /// Ingest data
    /// </summary>
    Task<int> IngestData<T>(List<T> data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ingest data to temp table, using original table as template
    /// </summary>
    Task<int> IngestTempData<T>(List<T> data, Guid ingestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Merge data from temp table to original
    /// </summary>
    Task<int> MergeTempData<T>(Guid ingestId, AuditValues auditValues, IEnumerable<string> matchColumns = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ingest data to temp table, using original table as template
    /// </summary>
    Task<int> IngestAndMergeData<T>(List<T> data, AuditValues auditValues, IEnumerable<string> matchColumns = null, CancellationToken cancellationToken = default);
}


/// <summary>
/// Definition of column to ingest
/// </summary>
internal class IngestColumnDefinition
{
    /// <summary>
    /// Column name
    /// </summary>
    internal string Name { get; set; }

    /// <summary>
    /// Db data type
    /// </summary>
    internal NpgsqlDbType Type { get; set; }


    /// <summary>
    /// Db data type
    /// </summary>
    internal string DbTypeName { get; set; }


    /// <summary>
    /// PropertyInfo
    /// </summary>
    internal PropertyInfo Property { get; set; }
}
