﻿using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;
using Altinn.AccessMgmt.Persistence.Core.Helpers;
using Altinn.AccessMgmt.Persistence.Core.Models;

namespace Altinn.AccessMgmt.Persistence.Core.Services;

/// <inheritdoc/>
public abstract class ExtendedRepository<T, TExtended> : BasicRepository<T>, IDbExtendedRepository<T, TExtended>
    where T : class, new()
    where TExtended : class, new()
{
    /// <inheritdoc/>
    protected ExtendedRepository(DbDefinitionRegistry dbDefinitionRegistry, IDbExecutor executor) : base(dbDefinitionRegistry, executor)
    {
    }

    /// <inheritdoc/>
    public async Task<TExtended> GetExtended(Guid id, RequestOptions options = null, CancellationToken cancellationToken = default, [CallerMemberName] string callerName = "")
    {
        var res = await GetExtended([new GenericFilter("Id", id)], options, cancellationToken, callerName);
        return res.Data.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<QueryResponse<TExtended>> GetExtended(RequestOptions options = null, CancellationToken cancellationToken = default, [CallerMemberName] string callerName = "")
    {
        return await GetExtended(filters: [], options, cancellationToken, callerName);
    }

    /// <inheritdoc/>
    public async Task<QueryResponse<TExtended>> GetExtended<TProperty>(Expression<Func<TExtended, TProperty>> property, TProperty value, RequestOptions options = null, CancellationToken cancellationToken = default, [CallerMemberName] string callerName = "")
    {
        string propertyName = ExtractPropertyInfo(property).Name;
        var filters = new List<GenericFilter>
        {
            new GenericFilter(propertyName, value!, FilterComparer.Equals)
        };
        return await GetExtended(filters, options, cancellationToken: cancellationToken, callerName);
    }

    /// <inheritdoc/>
    public async Task<QueryResponse<TExtended>> GetExtended(GenericFilterBuilder<TExtended> filter, RequestOptions options = null, CancellationToken cancellationToken = default, [CallerMemberName] string callerName = "")
    {
        return await GetExtended(filters: filter, options, cancellationToken, callerName);
    }

    /// <inheritdoc/>
    public async Task<QueryResponse<TExtended>> GetExtended(IEnumerable<GenericFilter> filters, RequestOptions options = null, CancellationToken cancellationToken = default, [CallerMemberName] string callerName = "")
    {
        options ??= new RequestOptions();
        filters ??= new List<GenericFilter>();

        var queryBuilder = definitionRegistry.GetQueryBuilder<T>();
        var query = queryBuilder.BuildExtendedSelectQuery(options, filters); //// TODO: Add CrossDefinition when needed
        var param = BuildFilterParameters(filters, options);

        return await executor.ExecuteQuery<TExtended>(query, param, callerName, cancellationToken: cancellationToken);
    }
}
