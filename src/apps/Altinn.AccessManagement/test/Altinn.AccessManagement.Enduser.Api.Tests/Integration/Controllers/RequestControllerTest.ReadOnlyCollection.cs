using Altinn.AccessManagement.TestUtils.Fixtures;

namespace Altinn.AccessManagement.Enduser.Api.Tests.Integration.Controllers;

/// <summary>
/// Shares a single <see cref="ApiFixture"/> across the read-only
/// <c>RequestController</c> test classes. Members must be
/// additive-seed-only, must not call <c>ConfigureServices</c> / configuration
/// helpers, and must not write rows other members read. Mutating classes
/// (<c>Create*</c>, <c>Reject*</c>, <c>Withdraw*</c>, <c>Approve*</c>, …) stay
/// on <see cref="Xunit.IClassFixture{TFixture}"/>.
/// </summary>
[CollectionDefinition(Name)]
public sealed class RequestReadOnlyCollection : ICollectionFixture<ApiFixture>
{
    /// <summary>Collection name referenced by member classes via <c>[Collection]</c>.</summary>
    public const string Name = "RequestController read-only";
}
