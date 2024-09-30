using System.Net;
using System.Text.Json;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Services;

/// <inheritdoc />
public class AccessPackagesService : IAccessPackageService
{
    private string AccessPackageFile { get; } = Path.Join(AppContext.BaseDirectory, "accesspackages.json");
    
    /// <inheritdoc />
    public async Task<IEnumerable<AccessPackageMetadataModel>> ListAccessPackagesMetadata(CancellationToken cancellationToken = default)
    {
        return await JsonSerializer.DeserializeAsync<IEnumerable<AccessPackageMetadataModel>>(File.OpenRead(AccessPackageFile), cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Manages Access Packages
/// </summary>
public interface IAccessPackageService
{
    /// <summary>
    /// Lists access package metadata 
    /// </summary>
    public Task<IEnumerable<AccessPackageMetadataModel>> ListAccessPackagesMetadata(CancellationToken cancellationToken = default);
}