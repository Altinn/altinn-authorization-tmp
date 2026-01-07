using Altinn.AccessMgmt.TestUtils.Utils;
using Altinn.Common.AccessToken.Services;
using Microsoft.IdentityModel.Tokens;

namespace Altinn.AccessMgmt.TestUtils.Mocks;

/// <summary>
/// tmp
/// </summary>
public class PublicSigningKeyProviderMock : IPublicSigningKeyProvider
{
    public Task<IEnumerable<SecurityKey>> GetSigningKeys(string issuer) =>
        Task.FromResult<IEnumerable<SecurityKey>>([TestTokenGenerator.SigningKey]);
}
