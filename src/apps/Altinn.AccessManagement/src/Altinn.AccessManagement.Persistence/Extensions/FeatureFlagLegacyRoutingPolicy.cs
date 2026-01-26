using Microsoft.FeatureManagement;

namespace Altinn.AccessManagement.Persistence.Extensions;

public sealed class FeatureFlagLegacyRoutingPolicy : ILegacyRoutingPolicy
{
    private readonly IFeatureManager _features;

    public FeatureFlagLegacyRoutingPolicy(IFeatureManager features) => _features = features;

    public Task<bool> UseLegacyAsync(string methodName, string space = "DelegationMetadata", string key = "Legacy", CancellationToken ct = default) => _features.IsEnabledAsync($"{space}.{methodName}.{key}");
}

public interface ILegacyRoutingPolicy
{
    Task<bool> UseLegacyAsync(string methodName, string space = "DelegationMetadata", string key = "Legacy", CancellationToken ct = default);
}
