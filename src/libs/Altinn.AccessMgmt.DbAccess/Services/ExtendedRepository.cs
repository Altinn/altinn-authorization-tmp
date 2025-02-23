using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.DbAccess.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessMgmt.DbAccess.Services;

/// <inheritdoc/>
public abstract class ExtendedRepository<T, TExtended> : BasicRepository<T>, IDbExtendedRepository<T, TExtended>
    where T : class, new()
    where TExtended : class, new()
{
    /// <summary>
    /// ExtendedRepository
    /// </summary>
    /// <param name="options">DbAccessConfig</param>
    /// <param name="connection">NpgsqlDataSource</param>
    /// <param name="dbConverter">DbConverter</param>
    protected ExtendedRepository(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter) { }

    /// <inheritdoc/>
    public async Task<TExtended?> GetExtended(Guid id, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var res = await GetExtended([new GenericFilter("Id", id)], options, cancellationToken);
        return res.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TExtended>> GetExtended(RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await GetExtended(filters: [], options, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TExtended>> GetExtended(GenericFilterBuilder<TExtended> filter, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await GetExtended(filters: filter, options, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TExtended>> GetExtended(IEnumerable<GenericFilter> filters, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new RequestOptions();
        filters ??= new List<GenericFilter>();

        var queryBuilder = new SqlQueryBuilder(Definition);
        var query = queryBuilder.BuildExtendedSelectQuery(options, filters);

        var parameterBuilder = new ParameterBuilder();
        var param = parameterBuilder.BuildFilterParameters(filters, options);

        var executor = new DbExecutor(connection, dbConverter);
        return await executor.ExecuteQuery<TExtended>(query, param, cancellationToken);
    }
}
