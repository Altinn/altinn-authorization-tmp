using Altinn.ApiClients.Maskinporten.Interfaces;

namespace Altinn.AccessManagement.Integration.Configuration
{
    /// <summary>
    /// Configuration settings for the Maskinporten client used to integrate with the ID-porten authorizations API
    /// </summary>
    public class IdPortenAuthorizationMaskinportenClientSettings : IMaskinportenSettings
    {
        /// <inheritdoc/>
        public string Environment { get; set; }

        /// <inheritdoc/>
        public string ClientId { get; set; }

        /// <inheritdoc/>
        public string Scope { get; set; }

        /// <inheritdoc/>
        public string EncodedJwk { get; set; }

        /// <inheritdoc/>
        public string Resource { get; set; }

        /// <inheritdoc/>
        public string CertificatePkcs12Path { get; set; }

        /// <inheritdoc/>
        public string CertificatePkcs12Password { get; set; }

        /// <inheritdoc/>
        public string CertificateStoreThumbprint { get; set; }

        /// <inheritdoc/>
        public string EncodedX509 { get; set; }

        /// <inheritdoc/>
        public string ConsumerOrgNo { get; set; }

        /// <inheritdoc/>
        public string EnterpriseUserName { get; set; }

        /// <inheritdoc/>
        public string EnterpriseUserPassword { get; set; }

        /// <inheritdoc/>
        public bool? ExhangeToAltinnToken { get; set; }

        /// <inheritdoc/>
        public string TokenExchangeEnvironment { get; set; }

        /// <inheritdoc/>
        public bool? UseAltinnTestOrg { get; set; }

        /// <inheritdoc/>
        public bool? EnableDebugLogging { get; set; }

        /// <inheritdoc/>
        public bool? OverwriteAuthorizationHeader { get; set; }
    }
}
