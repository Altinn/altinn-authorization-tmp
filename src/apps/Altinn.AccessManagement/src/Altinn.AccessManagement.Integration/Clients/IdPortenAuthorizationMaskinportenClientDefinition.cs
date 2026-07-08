using System.Text;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Models;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients;

/// <summary>
/// Maskinporten client definition for the ID-porten authorizations API integration.
/// Builds the client secrets from the base64-encoded JWK in <see cref="IdPortenAuthorizationMaskinportenClientSettings"/>.
/// </summary>
public class IdPortenAuthorizationMaskinportenClientDefinition : IClientDefinition
{
    /// <inheritdoc/>
    public IMaskinportenSettings ClientSettings { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="IdPortenAuthorizationMaskinportenClientDefinition"/> class
    /// </summary>
    /// <param name="clientSettings">Maskinporten client settings</param>
    public IdPortenAuthorizationMaskinportenClientDefinition(IOptions<IdPortenAuthorizationMaskinportenClientSettings> clientSettings) => ClientSettings = clientSettings.Value;

    /// <inheritdoc/>
    public Task<ClientSecrets> GetClientSecrets()
    {
        ClientSecrets clientSecrets = new ClientSecrets();

        byte[] bytesFromBase64Jwk = Convert.FromBase64String(ClientSettings.EncodedJwk);
        string jwkJson = Encoding.UTF8.GetString(bytesFromBase64Jwk);
        clientSecrets.ClientKey = new Microsoft.IdentityModel.Tokens.JsonWebKey(jwkJson);
        return Task.FromResult(clientSecrets);
    }
}
