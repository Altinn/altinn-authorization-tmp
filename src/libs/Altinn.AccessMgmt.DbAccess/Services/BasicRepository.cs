using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.DbAccess.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessMgmt.DbAccess.Services;

/// <inheritdoc/>
public abstract class BasicRepository<T> : IDbBasicRepository<T>
    where T : class, new()
{
    protected readonly DbAccessConfig config;
    protected readonly NpgsqlDataSource connection;
    protected readonly IDbConverter dbConverter;

    public BasicRepository(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter)
    {
        config = options.Value;
        this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
        this.dbConverter = dbConverter;
    }

    /// <summary>
    /// Definition
    /// </summary>
    public DbDefinition Definition { get { return DefinitionStore.Definition<T>(); } }

    /// <inheritdoc/>
    public GenericFilterBuilder<T> CreateFilterBuilder() { return new GenericFilterBuilder<T>(); }

    #region Read

    /// <inheritdoc/>
    public async Task<T?> Get(Guid id, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var res = await Get(new List<GenericFilter>() { new GenericFilter("id", id) }, options, cancellationToken: cancellationToken);
        return res.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get<TProperty>(Expression<Func<T, TProperty>> property, TProperty value, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        string propertyName = ExtractPropertyInfo(property).Name;
        var filters = new List<GenericFilter>
        {
            new GenericFilter(propertyName, value, FilterComparer.Equals)
        };
        return await Get(filters, options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get(RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await Get(filters: new List<GenericFilter>(), options: options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get(GenericFilterBuilder<T> filterBuilder, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await Get(filters: filterBuilder, options: options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get(IEnumerable<GenericFilter> filters, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new RequestOptions();
        filters ??= new List<GenericFilter>();

        var queryBuilder = new SqlQueryBuilder(Definition);
        var query = queryBuilder.BuildBasicSelectQuery(options, filters);

        var parameterBuilder = new ParameterBuilder();
        var param = parameterBuilder.BuildFilterParameters(filters, options);

        var dbExec = new DbExecutor(connection, dbConverter);
        return await dbExec.ExecuteQuery<T>(query, param, cancellationToken: cancellationToken);
    }

    private PropertyInfo ExtractPropertyInfo<TLocal, TProperty>(Expression<Func<TLocal, TProperty>> expression)
    {
        MemberExpression? memberExpression = expression.Body switch
        {
            MemberExpression member => member,
            UnaryExpression { Operand: MemberExpression member } => member,
            _ => null
        };

        return memberExpression?.Member as PropertyInfo ?? throw new ArgumentException($"Expression '{expression}' does not refer to a valid property.");
    }
    #endregion

    #region Write

    /// <inheritdoc/>
    public async Task<int> Ingest(List<T> data, CancellationToken cancellationToken = default)
    {
        var importer = new BulkImporter<T>(connection, Definition);
        return await importer.Ingest(data, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Create(T entity, CancellationToken cancellationToken = default)
    {
        var parameterBuilder = new ParameterBuilder();
        var param = parameterBuilder.BuildParameters(entity);

        var queryBuilder = new SqlQueryBuilder(Definition);
        string query = queryBuilder.BuildInsertQuery(param);

        var dbExec = new DbExecutor(connection, dbConverter);
        return await dbExec.ExecuteCommand(query, param, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Upsert(T entity, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        var parameterBuilder = new ParameterBuilder();
        var param = parameterBuilder.BuildParameters(entity);

        var queryBuilder = new SqlQueryBuilder(Definition);
        string query = queryBuilder.BuildUpsertQuery(param);

        var dbExec = new DbExecutor(connection, dbConverter);
        return await dbExec.ExecuteCommand(sb.ToString(), param, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, T entity, CancellationToken cancellationToken = default)
    {
        var parameterBuilder = new ParameterBuilder();
        var param = parameterBuilder.BuildParameters(entity);

        return await Update(id: id, parameters: param, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, List<GenericParameter> parameters, CancellationToken cancellationToken = default)
    {
        var param = parameters.Select(t => new NpgsqlParameter(t.Key, t.Value)).ToList();
        return await Update(id: id, parameters: param, cancellationToken: cancellationToken);
    }

    private async Task<int> Update(Guid id, List<NpgsqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        var queryBuilder = new SqlQueryBuilder(Definition);
        string query = queryBuilder.BuildUpdateQuery(parameters);

        parameters.Add(new NpgsqlParameter("_id", id));

        var dbExec = new DbExecutor(connection, dbConverter);
        return await dbExec.ExecuteCommand(query, parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var queryBuilder = new SqlQueryBuilder(Definition);
        string query = queryBuilder.BuildDeleteQuery();

        var dbExec = new DbExecutor(connection, dbConverter);
        return await dbExec.ExecuteCommand(query, [new NpgsqlParameter("_id", id)], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> CreateTranslation(T obj, string language, CancellationToken cancellationToken = default)
    {
        if (!Definition.HasTranslation)
        {
            return 0;
        }

        var parameterBuilder = new ParameterBuilder();
        var parameters = parameterBuilder.BuildTranslationParameters(obj);
        parameters.Add(new NpgsqlParameter("Language", language));

        var queryBuilder = new SqlQueryBuilder(Definition);
        string query = queryBuilder.BuildInsertQuery(parameters, forTranslation: true);

        var dbExec = new DbExecutor(connection, dbConverter);
        return await dbExec.ExecuteCommand(query, parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> UpdateTranslation(Guid id, T obj, string language, CancellationToken cancellationToken = default)
    {
        if (!Definition.HasTranslation)
        {
            return 0;
        }

        var parameterBuilder = new ParameterBuilder();
        var parameters = parameterBuilder.BuildTranslationParameters(obj);

        var queryBuilder = new SqlQueryBuilder(Definition);
        string query = queryBuilder.BuildUpdateQuery(parameters, forTranslation: true);

        parameters.Add(new NpgsqlParameter("_language", language));
        parameters.Add(new NpgsqlParameter("_id", id));

        var dbExec = new DbExecutor(connection, dbConverter);
        return await dbExec.ExecuteCommand(query, parameters, cancellationToken: cancellationToken);
    }

    #endregion

}
