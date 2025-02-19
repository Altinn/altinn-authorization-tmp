using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.DbAccess.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessMgmt.DbAccess.Services;

/// <inheritdoc/>
public abstract class CrossRepository<T, TExtended, TA, TB> : ExtendedRepository<T, TExtended>, IDbCrossRepository<T, TExtended, TA, TB>
    where T : class, new()
    where TExtended : class, new()
    where TA : class, new()
    where TB : class, new()
{
    /// <inheritdoc/>
    protected CrossRepository(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter)
        : base(options, connection, dbConverter) { }

    /// <inheritdoc/>
    public async Task<IEnumerable<TA>> GetA(Guid id, RequestOptions options, List<GenericFilter> filters, CancellationToken cancellationToken)
        => await GetCrossReferencedEntities<TA>(id, options, filters, cancellationToken);

    /// <inheritdoc/>
    public async Task<IEnumerable<TB>> GetB(Guid id, RequestOptions options, List<GenericFilter> filters, CancellationToken cancellationToken)
        => await GetCrossReferencedEntities<TB>(id, options, filters, cancellationToken);

    /// <summary>
    /// Retrieves entities from a cross-reference table based on the given type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to retrieve.</typeparam>
    /// <param name="id">The cross-reference identifier.</param>
    /// <param name="options">Query request options.</param>
    /// <param name="filters">Optional filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of entities of type <typeparamref name="TEntity"/>.</returns>
    private async Task<IEnumerable<TEntity>> GetCrossReferencedEntities<TEntity>(
        Guid id, RequestOptions options, List<GenericFilter> filters, CancellationToken cancellationToken)
        where TEntity : class, new()
    {
        // Get the definition for the entity type
        var def = DefinitionStore.TryGetDefinition<TEntity>() ?? DefinitionStore.TryGetBaseDefinition<TEntity>();

        if (def == null)
        {
            throw new Exception($"Definition not found for {typeof(TEntity).Name}");
        }

        bool isExtended = def != DefinitionStore.TryGetDefinition<TEntity>();

        options ??= new RequestOptions();
        filters ??= new List<GenericFilter>();

        var queryBuilder = new SqlQueryBuilder(def);
        string query = isExtended
            ? queryBuilder.BuildExtendedSelectQuery(options, filters, Definition.CrossRelation)
            : queryBuilder.BuildBasicSelectQuery(options, filters, Definition.CrossRelation);

        var parameterBuilder = new ParameterBuilder();
        var param = parameterBuilder.BuildFilterParameters(filters, options);

        var dbExec = new DbExecutor(connection, dbConverter);
        return await dbExec.ExecuteQuery<TEntity>(query, param, cancellationToken);
    }
}
