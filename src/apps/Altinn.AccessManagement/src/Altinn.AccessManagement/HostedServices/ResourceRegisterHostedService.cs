using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.Authorization.Host.Lease;
using MassTransit.Configuration;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessManagement;

/// <summary>
/// ResourceRegisterHostedService
/// </summary>
/// <param name="lease">Lease provider for distributed locking.</param>
/// <param name="logger">Register integration service</param>
/// <param name="options">ResourceRegisterImportConfig</param>
/// <param name="providerService">IProviderService</param>
/// <param name="resourceTypeService">IResourceTypeService</param>
/// <param name="resourceGroupService">IResourceGroupService</param>
/// <param name="resourceService">IResourceService</param>
/// <param name="roleService">IRoleService</param>
/// <param name="roleResourceService">IRoleResourceService</param>
/// <param name="packageService">IPackageService</param>
/// <param name="packageResourceService">IPackageResourceService</param>
public partial class ResourceRegisterHostedService(
    IAltinnLease lease,
    ILogger<ResourceRegisterHostedService> logger,
    IOptions<ResourceRegisterImportConfig> options,
    IProviderService providerService,
    IResourceTypeService resourceTypeService,
    IResourceGroupService resourceGroupService,
    IResourceService resourceService,
    IRoleService roleService,
    IRoleResourceService roleResourceService,
    IPackageService packageService,
    IPackageResourceService packageResourceService
    ) : IHostedService, IDisposable
{
    private readonly IAltinnLease _lease = lease;
    private readonly ILogger<ResourceRegisterHostedService> _logger = logger;
    private readonly IProviderService providerService = providerService;
    private readonly IResourceTypeService resourceTypeService = resourceTypeService;
    private readonly IResourceGroupService resourceGroupService = resourceGroupService;
    private readonly IResourceService resourceService = resourceService;
    private readonly IRoleService roleService = roleService;
    private readonly IRoleResourceService roleResourceService = roleResourceService;
    private readonly IPackageService packageService = packageService;
    private readonly IPackageResourceService packageResourceService = packageResourceService;
    private readonly ResourceRegisterImportConfig _config = options.Value;
    private readonly HttpClient _client = new HttpClient();
    private Timer _timer = null;
    private readonly CancellationTokenSource _stop = new();
    private static readonly ActivitySource _activitySource = new ActivitySource("Altinn.Authorization.AccessMgmt.ResourceImporter");

    private List<Provider> Providers { get; set; }

    private List<ResourceType> ResourceTypes { get; set; }

    private List<Role> Roles { get; set; }

    private List<Package> Packages { get; set; }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("StartAsync");

        _logger.LogInformation("Starting register sync process");
        activity?.SetTag("Status", "Starting");

        var timerInterval = TimeSpan.FromMinutes(_config.TimerIntervalMinutes));

        _timer = new Timer(SyncDispatcher, _stop.Token, TimeSpan.Zero, timerInterval);

        activity?.SetTag("TimerIntervalMinutes", timerInterval.ToString());
        activity?.SetTag("Status", "Started");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Dispatches the register synchronization process in a separate task.
    /// </summary>
    /// <param name="state">Cancellation token for stopping execution.</param>
    private void SyncDispatcher(object state)
    {
        using var activity = _activitySource.StartActivity("SyncDispatcher");

        var cancellationToken = (CancellationToken)state;
        _logger.LogInformation("Starting sync dispatcher");

        try
        {
            if (_config.UseStream)
            {
                activity?.SetTag("SyncType", "Stream");
                SyncStream(cancellationToken).Wait();
            }
            else
            {
                activity?.SetTag("SyncType", "All");
                SyncAll(cancellationToken).Wait();
            }

            activity?.SetTag("Status", "Completed");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Sync failed");
            _logger.LogError(ex, "SyncDispatcher encountered an error");
        }
    }

    private async Task LoadCache(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("LoadCache");
        _logger.LogInformation("Loading cache data");

        try
        {
            Providers = [.. await providerService.Get(cancellationToken: cancellationToken)];
            ResourceTypes = [.. await resourceTypeService.Get(cancellationToken: cancellationToken)];
            Roles = [.. await roleService.Get(cancellationToken: cancellationToken)];
            Packages = [.. await packageService.Get(cancellationToken: cancellationToken)];

            activity?.SetTag("CacheStatus", "Loaded");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Cache load failed");
            _logger.LogError(ex, "Failed to load cache");
        }
    }

    /// <summary>
    /// Synchronizes rawResource Data by first acquiring a remote lease and streaming register entries.
    /// Returns if lease is already taken.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task SyncAll(CancellationToken cancellationToken)
    {
        await using var ls = await _lease.TryAquireNonBlocking<LeaseContent>("access_management_register_sync", cancellationToken);
        if (!ls.HasLease || cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Lease not acquired or cancellation requested, skipping sync");
            return;
        }

        using var activity = _activitySource.StartActivity("Sync");

        try
        {
            await LoadCache(cancellationToken);

            _client.BaseAddress = new Uri(_config.BaseUrl);

            activity.AddEvent(new ActivityEvent("Get rawResources from resourceregistry"));
            var rawResources = await _client.GetFromJsonAsync<List<RawResource>>("/resourceregistry/api/v1/rawResource/resourcelist", cancellationToken) ?? [];

            foreach (var rawResource in rawResources)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Lease not acquired or cancellation requested, skipping sync");
                    return;
                }

                var resource = await UpsertResource(rawResource, cancellationToken);
                await UpsertSubjects(resource, cancellationToken);
            }

            activity?.SetTag("Status", "Completed");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Sync failed");
            _logger.LogError(ex, "Sync process encountered an error");
        }
    }

    /// <summary>
    /// Synchronizes rawResource Data by first acquiring a remote lease and streaming register entries.
    /// Returns if lease is already taken.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task SyncStream(CancellationToken cancellationToken)
    {
        await using var ls = await _lease.TryAquireNonBlocking<LeaseContent>("access_management_register_sync", cancellationToken);
        if (!ls.HasLease || cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Lease not acquired or cancellation requested, skipping sync");
            return;
        }

        using var activity = _activitySource.StartActivity("Sync");
        activity?.SetTag("LeaseAcquired", true);

        await LoadCache(cancellationToken);

        try
        {
            var url = string.IsNullOrEmpty(ls.Data.NextPageLink)
                ? $"{_config.BaseUrl}/resourceregistry/api/v1/resource/updated"
                : ls.Data.NextPageLink;

            _logger.LogInformation("Fetching updated resources from: {Url}", url);
            var updates = await _client.GetFromJsonAsync<StreamResult>(url, cancellationToken);
            activity?.SetTag("UpdatesCount", updates.Data.Count);

            foreach (var update in updates.Data)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Sync process cancelled while processing resource: {ResourceUrn}", update.ResourceUrn);
                    activity?.SetTag("Status", "Cancelled");
                    return;
                }

                var resourceUrn = SimpleUrnSplit(update.ResourceUrn);
                if (string.IsNullOrEmpty(resourceUrn.Value))
                {
                    _logger.LogWarning("Invalid URN format for resource: {ResourceUrn}", update.ResourceUrn);
                    continue;
                }

                try
                {
                    var rawResource = await _client.GetFromJsonAsync<RawResource>($"{_config.BaseUrl}/resourceregistry/api/v1/resource/{resourceUrn.Value}", cancellationToken);
                    var resource = await UpsertResource(rawResource, cancellationToken);
                    await UpsertSubjects(resource, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing resource: {ResourceUrn}", update.ResourceUrn);
                    activity?.AddEvent(new ActivityEvent($"Error processing resource {update.ResourceUrn}"));
                }
            }

            if (updates.Links.ContainsKey("next") && !string.IsNullOrEmpty(updates.Links["next"]))
            {
                _logger.LogInformation("Next page link found, updating lease with next page: {NextPageLink}", updates.Links["next"]);
                await _lease.Put(ls, new() { NextPageLink = updates.Links["next"] }, cancellationToken);
            }
            else
            {
                _logger.LogInformation("No more pages, sync completed");
                activity?.SetTag("Status", "Completed");
                return;
            }

            await _lease.RefreshLease(ls, cancellationToken);
            activity?.SetTag("Status", "Lease refreshed");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Sync failed");
            _logger.LogError(ex, "Sync process encountered an error");
        }
    }

    private (string Type, string Value) SimpleUrnSplit(string urn)
    {
        if (string.IsNullOrWhiteSpace(urn))
        {
            _logger.LogWarning("Attempted to split an empty or null URN.");
            return (string.Empty, string.Empty);
        }

        var split = urn.Split(':');

        if (split.Length < 2)
        {
            _logger.LogWarning("Invalid URN format: {Urn}", urn);
            return (urn, string.Empty);
        }

        return (string.Join(':', split[..^1]), split[^1]);
    }

    private async Task<Resource> UpsertResource(RawResource rawResource, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("UpsertResource");
        activity?.SetTag("ResourceId", rawResource.Identifier);
        activity?.SetTag("ResourceType", rawResource.ResourceType);

        var result = await resourceService.Get(t => t.RefId, rawResource.Identifier, cancellationToken: cancellationToken);

        if (result.Any())
        {
            var resource = result.First();
            activity?.SetTag("Result", "ExistingResourceFound");

            if (!resource.Name.Equals(rawResource.Title.Nb))
            {
                using var updateActivity = _activitySource.StartActivity("UpdateResourceName");

                _logger.LogInformation("Updating Resource Name: ResourceId={ResourceId}, OldName={OldName}, NewName={NewName}", resource.Id, resource.Name, rawResource.Title.Nb);

                updateActivity?.SetTag("ResourceId", resource.Id);
                updateActivity?.SetTag("OldName", resource.Name);
                updateActivity?.SetTag("NewName", rawResource.Title.Nb);
                updateActivity?.AddEvent(new ActivityEvent("Resource name updated"));

                resource.Name = rawResource.Title.Nb;
                await resourceService.ExtendedRepo.Upsert(resource.Id, resource, cancellationToken: cancellationToken);
            }

            return resource;
        }

        var provider = await UpsertProvider(rawResource.HasCompetentAuthority.Name.ToString(), rawResource.HasCompetentAuthority.Organization, cancellationToken: cancellationToken);
        var resourceGroup = await UpsertResourceGroup(rawResource.ResourceType, provider.Id, cancellationToken: cancellationToken);
        var resourceType = await UpsertResourceType(rawResource.ResourceType, cancellationToken: cancellationToken);

        var newObj = new Resource
        {
            Id = Guid.NewGuid(),
            RefId = rawResource.Identifier,
            ProviderId = provider.Id,
            GroupId = resourceGroup.Id,
            Name = rawResource.Title.Nb,
            TypeId = resourceType.Id
        };

        await resourceService.Create(newObj, cancellationToken: cancellationToken);
        activity?.SetTag("Result", "Created");

        return newObj;
    }

    private async Task UpsertSubjects(Resource resource, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("UpsertSubjects");
        activity?.SetTag("ResourceId", resource.RefId);

        try
        {
            var response = await _client.GetAsync($"resourceregistry/api/v1/rawResource/{resource.RefId}/policy/subjects", cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch subjects for resource {ResourceId}. HTTP Status: {StatusCode}", resource.RefId, response.StatusCode);
                return;
            }

            var subjectResult = JsonSerializer.Deserialize<SubjectResult>(await response.Content.ReadAsStringAsync());
            if (subjectResult == null)
            {
                _logger.LogWarning("No subjects found for resource {ResourceId}", resource.RefId);
                return;
            }

            foreach (var subject in subjectResult.Data)
            {
                switch (subject.Type)
                {
                    case "Urn:altinn:rolecode":
                        await UpsertRoleResource(subject.Value, resource.Id, cancellationToken: cancellationToken);
                        break;
                    case "Urn:altinn:accesspackage":
                        await UpsertPackageResource(subject.Value, resource.Id, cancellationToken: cancellationToken);
                        break;
                    default:
                        _logger.LogWarning("Unknown subject type {SubjectType} for resource {ResourceId}", subject.Type, resource.RefId);
                        break;
                }
            }

            activity?.SetTag("Status", "Completed");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "UpsertSubjects failed");
            _logger.LogError(ex, "Error occurred while fetching subjects for resource {ResourceId}", resource.RefId);
        }
    }

    private async Task UpsertPackageResource(string packageUrn, Guid resourceId, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("UpsertPackageResource");
        activity?.SetTag("PackageUrn", packageUrn);
        activity?.SetTag("ResourceId", resourceId);

        var package = Packages.FirstOrDefault(t => t.Urn == packageUrn);
        if (package == null)
        {
            _logger.LogWarning("Package not found: {PackageUrn}", packageUrn);
            return;
        }

        var filter = packageResourceService.CreateFilterBuilder<PackageResource>();
        filter.Add(t => t.PackageId, package.Id);
        filter.Add(t => t.ResourceId, resourceId);

        var pr = await packageResourceService.Get(filter, cancellationToken: cancellationToken);
        if (pr.Any())
        {
            activity?.SetTag("Result", "AlreadyExists");
            return;
        }

        await packageResourceService.Create(
            new PackageResource
            {
                Id = Guid.NewGuid(),
                PackageId = package.Id,
                ResourceId = resourceId
            },
            cancellationToken: cancellationToken);

        activity?.SetTag("Result", "Created");
    }

    private async Task UpsertRoleResource(string roleCode, Guid resourceId, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("UpsertRoleResource");
        activity?.SetTag("RoleCode", roleCode);
        activity?.SetTag("ResourceId", resourceId);

        var role = Roles.FirstOrDefault(t => t.Code.Equals(roleCode, StringComparison.OrdinalIgnoreCase));
        if (role == null)
        {
            _logger.LogWarning("Role not found: {RoleCode}", roleCode);
            return;
        }

        var filter = roleResourceService.CreateFilterBuilder<RoleResource>();
        filter.Add(t => t.RoleId, role.Id);
        filter.Add(t => t.ResourceId, resourceId);

        var rr = await roleResourceService.Get(filter, cancellationToken: cancellationToken);
        if (rr.Any())
        {
            activity?.SetTag("Result", "AlreadyExists");
            return;
        }

        await roleResourceService.Create(
            new RoleResource
            {
                Id = Guid.NewGuid(),
                RoleId = role.Id,
                ResourceId = resourceId
            }, 
            cancellationToken: cancellationToken);

        activity?.SetTag("Result", "Created");
    }

    private async Task<ResourceType> UpsertResourceType(string name, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("UpsertResourceType");
        activity?.SetTag("ResourceTypeName", name);

        name = string.IsNullOrEmpty(name) ? "Unknown" : name;

        var result = await resourceTypeService.Get(t => t.Name, name, cancellationToken: cancellationToken);

        if (result.Any())
        {
            activity?.SetTag("Result", "ExistingResourceTypeFound");
            return result.First();
        }

        var newObj = new ResourceType
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        await resourceTypeService.Create(newObj, cancellationToken: cancellationToken);
        activity?.SetTag("Result", "Created");

        return newObj;
    }

    private async Task<Provider> UpsertProvider(string name, string refId, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("UpsertProvider");
        activity?.SetTag("ProviderName", name);
        activity?.SetTag("ProviderRefId", refId);

        name = string.IsNullOrEmpty(name) ? "Unknown" : name;
        refId = string.IsNullOrEmpty(refId) ? "N/A" : refId;

        var filter = providerService.CreateFilterBuilder<Provider>();
        filter.Add(t => t.Name, name);
        //// filter.Add(t => t.RefId, refId);

        var res = await providerService.Get(filter, cancellationToken: cancellationToken);

        if (res.Any())
        {
            activity?.SetTag("Result", "ExistingProviderFound");
            return res.First();
        }

        var newObj = new Provider
        {
            Id = Guid.NewGuid(),
            Name = name,
            RefId = refId
        };

        await providerService.Create(newObj, cancellationToken: cancellationToken);
        activity?.SetTag("Result", "Created");

        return newObj;
    }

    private async Task<ResourceGroup> UpsertResourceGroup(string name, Guid providerId, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("UpsertResourceGroup");
        activity?.SetTag("Verbosity", "Low");
        activity?.SetTag("ResourceGroupName", name);
        activity?.SetTag("ProviderId", providerId);

        name = string.IsNullOrEmpty(name) ? "Unknown" : name;

        try
        {
            var filter = resourceGroupService.CreateFilterBuilder<ResourceGroup>();
            filter.Add(t => t.Name, name);
            filter.Add(t => t.ProviderId, providerId);

            var result = await resourceGroupService.Get(filter, cancellationToken: cancellationToken);

            if (result.Any())
            {
                activity?.SetTag("Result", "ExistingResourceGroupFound");
                return result.First();
            }

            var newObj = new ResourceGroup { Id = Guid.NewGuid(), Name = name, ProviderId = providerId };

            await resourceGroupService.Create(newObj, cancellationToken: cancellationToken);
            activity?.SetTag("Result", "NewResourceGroupCreated");

            return newObj;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpsertResourceGroup");
            throw;
        }
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("StopAsync");

        _logger.LogInformation("Stopping register sync process");
        activity?.SetTag("Status", "Stopping");

        try
        {
            _timer?.Change(Timeout.Infinite, 0);
            _stop.Cancel();

            _logger.LogInformation("Register sync process stopped");
            activity?.SetTag("Status", "Stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while stopping register sync process");
            activity?.SetStatus(ActivityStatusCode.Error, "Stop failed");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged rawResources.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stop?.Dispose();
            _timer?.Dispose();
        }
    }
    
    /// <summary>
    /// Represents lease data
    /// </summary>
    public class LeaseContent
    {
        /// <summary>
        /// The URL of the next update of Data.
        /// </summary>
        public string NextPageLink { get; set; }
    }

    public class StreamResult
    {
        [JsonPropertyName("links")]
        public Dictionary<string, string> Links { get; set; }

        [JsonPropertyName("data")]
        public List<ResourceUpdate> Data { get; set; }
    }

    public class ResourceUpdate
    {
        [JsonPropertyName("subjectUrn")]
        public string SubjectUrn { get; set; }
        [JsonPropertyName("resourceUrn")]
        public string ResourceUrn { get; set; }
        [JsonPropertyName("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }
        [JsonPropertyName("deleted")]
        public bool IsDeleted { get; set; }
    }
}

/// <summary>
/// Configuration for Resource import
/// </summary>
public class ResourceRegisterImportConfig
{
    /// <summary>
    /// BaseUrl for resourceregister api
    /// </summary>
    public string BaseUrl { get; set; } = "https://platform.altinn.no";

    /// <summary>
    /// Interval to run import (in minutes)
    /// </summary>
    public int TimerIntervalMinutes { get; set; } = 2;

    /// <summary>
    /// If importer uses /updates (stream) or /all
    /// </summary>
    public bool UseStream { get; set; } = false;
}

/// <summary>
/// Result model for Subject query
/// </summary>
internal class ResourceUpdateResult
{
    /// <summary>
    /// Data content
    /// </summary>
    [JsonPropertyName("data")]
    public List<Subject> Data { get; set; }
    /// <summary>
    /// Data content
    /// </summary>
    [JsonPropertyName("data")]
    public List<Subject> Data { get; set; }
}

/// <summary>
/// Result model for Subject query
/// </summary>
internal class SubjectResult
{
    /// <summary>
    /// Data content
    /// </summary>
    [JsonPropertyName("data")]
    public List<Subject> Data { get; set; }
}

/// <summary>
/// Result from Subject query
/// </summary>
internal class Subject
{
    /// <summary>
    /// Urn Type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// Subject Value
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; }

    /// <summary>
    /// Complete Urn Type+Value
    /// </summary>
    [JsonPropertyName("urn")]
    public string Urn { get; set; }
}

/// <summary>
/// LanguageText
/// </summary>
internal class LanguageText
{
    /// <summary>
    /// English
    /// </summary>
    public string En { get; set; }

    /// <summary>
    /// Bokmål
    /// </summary>
    public string Nb { get; set; }

    /// <summary>
    /// Nynorsk
    /// </summary>
    public string Nn { get; set; }

    /// <summary>
    /// Returns first available
    /// Nb ?? En ?? Nn ?? ""
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Nb ?? En ?? Nn ?? "";
    }

}

/// <summary>
/// ResourceReference
/// </summary>
internal class ResourceReference
{
    /// <summary>
    /// ReferenceSource
    /// </summary>
    public string ReferenceSource { get; set; }

    /// <summary>
    /// Reference
    /// </summary>
    public string Reference { get; set; }

    /// <summary>
    /// ReferenceType
    /// </summary>
    public string ReferenceType { get; set; }
}

/// <summary>
/// HasCompetentAuthority
/// </summary>
internal class HasCompetentAuthority
{
    /// <summary>
    /// Name
    /// </summary>
    public LanguageText Name { get; set; }

    /// <summary>
    /// Organization
    /// </summary>
    public string Organization { get; set; }

    /// <summary>
    /// Orgcode
    /// </summary>
    public string Orgcode { get; set; }
}

/// <summary>
/// AuthorizationReference
/// </summary>
internal class AuthorizationReference
{
    /// <summary>
    /// Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Value
    /// </summary>
    public string Value { get; set; }
}

/// <summary>
/// RawResource
/// </summary>
internal class RawResource
{
    /// <summary>
    /// Identifier
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// Title
    /// </summary>
    public LanguageText Title { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public LanguageText Description { get; set; }

    /// <summary>
    /// RightDescription
    /// </summary>
    public LanguageText RightDescription { get; set; }

    /// <summary>
    /// Homepage
    /// </summary>
    public string Homepage { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// IsPartOf
    /// </summary>
    public string IsPartOf { get; set; }

    /// <summary>
    /// ResourceReferences
    /// </summary>
    public List<ResourceReference> ResourceReferences { get; set; }

    /// <summary>
    /// Delegable
    /// </summary>
    public bool Delegable { get; set; }

    /// <summary>
    /// Visible
    /// </summary>
    public bool Visible { get; set; }

    /// <summary>
    /// HasCompetentAuthority
    /// </summary>
    public HasCompetentAuthority HasCompetentAuthority { get; set; }

    /// <summary>
    /// AccessListMode
    /// </summary>
    public string AccessListMode { get; set; }

    /// <summary>
    /// SelfIdentifiedUserEnabled
    /// </summary>
    public bool SelfIdentifiedUserEnabled { get; set; }

    /// <summary>
    /// EnterpriseUserEnabled
    /// </summary>
    public bool EnterpriseUserEnabled { get; set; }

    /// <summary>
    /// ResourceType
    /// </summary>
    public string ResourceType { get; set; }

    /// <summary>
    /// AuthorizationReference
    /// </summary>
    public List<AuthorizationReference> AuthorizationReference { get; set; }
}
