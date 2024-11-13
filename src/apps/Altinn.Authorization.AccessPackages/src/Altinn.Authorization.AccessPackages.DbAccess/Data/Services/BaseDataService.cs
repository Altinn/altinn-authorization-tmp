using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Services;

public static class Logs
{
    public static ILoggerFactory LoggerFactory { get; set; } = new LoggerFactory();

}

/// <inheritdoc/>
public class BaseDataService<T> : IDbBasicDataService<T>
{
    /// <summary>
    /// Extended repo
    /// </summary>
    public IDbBasicRepo<T> Repo { get; }
    public ILogger Logger { get; }

    /// <summary>
    /// Base data service
    /// </summary>
    /// <param name="repo">ExtendedRepo</param>
    public BaseDataService(IDbBasicRepo<T> repo)
    {
        Repo = repo;
        Logger = Logs.LoggerFactory.CreateLogger(typeof(BaseDataService<T>));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get(RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var a = Telemetry.StartActivity<T>("Get");
        return await Repo.Get(parameters: new List<GenericFilter>(), options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<T?> Get(Guid id, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var a = Telemetry.StartActivity<T>("Get");
        try
        {
            var res = await Repo.Get([new GenericFilter("Id", id)], options, cancellationToken: cancellationToken);
            if (res != null)
            {
                return res.First();
            }

            return default;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get(string property, Guid value, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var a = Telemetry.StartActivity<T>("Get");
        return await Repo.Get([new GenericFilter(property, value)], options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get(string property, int value, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var a = Telemetry.StartActivity<T>("Get");
        return await Repo.Get([new GenericFilter(property, value)], options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get(string property, string value, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var a = Telemetry.StartActivity<T>("Get");
        return await Repo.Get([new GenericFilter(property, value)], options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get(Dictionary<string, object> parameters, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var a = Telemetry.StartActivity<T>("Get");
        var param = new List<GenericFilter>();
        foreach (var p in parameters)
        {
            param.Add(new GenericFilter(p.Key, p.Value));
        }

        return await Repo.Get(param, options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Search(string term, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var a = Telemetry.StartActivity<T>("Search");
        return await Repo.Get([new GenericFilter("Name", term, comparer: DbOperators.Contains)], options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public Task<(IEnumerable<T> Data, PagedResult PageInfo)> SearchPaged(string term, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var a = Telemetry.StartActivity<T>("SearchPaged");
        return Repo.Search(term, options ?? new RequestOptions(), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Create(List<T> entities, CancellationToken cancellationToken = default)
    {
        int count = 0;
        foreach (var entity in entities)
        {
            count += await Create(entity, cancellationToken: cancellationToken);
        }

        return count;
    }

    /// <inheritdoc/>
    public async Task<int> Create(T entity, CancellationToken cancellationToken = default)
    {
        return await Repo.Create(entity, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, T entity, CancellationToken cancellationToken = default)
    {
        return await Repo.Update(id, entity, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, string property, Guid value, CancellationToken cancellationToken = default)
    {
        return await Repo.Update(id, [new GenericParameter(property, value)], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, string property, string value, CancellationToken cancellationToken = default)
    {
        return await Repo.Update(id, [new GenericParameter(property, value)], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, string property, int value, CancellationToken cancellationToken = default)
    {
        return await Repo.Update(id, [new GenericParameter(property, value)], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        return await Repo.Delete(id, cancellationToken);
    }
}
