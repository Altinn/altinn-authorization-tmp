using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Microsoft.IdentityModel.Tokens;

namespace Altinn.Platform.Authorization.IntegrationTests.Util;

/// <summary>
/// Provides deterministic, in-memory X.509 certificates for use in integration tests.
/// Eliminates the need for certificate files checked into the repository.
/// </summary>
internal static class TestCertificates
{
    private static readonly Lazy<(X509Certificate2 Cert, RSA Key)> _default = new(CreateCertificate);

    /// <summary>
    /// Self-signed certificate used for signing and verifying test JWTs.
    /// Replaces the former <c>selfSignedTestCertificate.pfx</c> / <c>.cer</c> files.
    /// </summary>
    public static X509Certificate2 Default => _default.Value.Cert;

    /// <summary>
    /// Signing credentials wrapping <see cref="Default"/> for token generation.
    /// </summary>
    public static SigningCredentials SigningCredentials =>
        new X509SigningCredentials(Default, SecurityAlgorithms.RsaSha256);

    /// <summary>
    /// Security key wrapping <see cref="Default"/> for token validation.
    /// </summary>
    public static SecurityKey SecurityKey => new X509SecurityKey(Default);

    private static (X509Certificate2, RSA) CreateCertificate()
    {
        var rsa = RSA.Create(2048);

        var req = new CertificateRequest(
            "CN=AuthorizationTest",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        req.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

        var temp = req.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(1));

        var cert = X509CertificateLoader.LoadPkcs12(
            temp.Export(X509ContentType.Pfx),
            password: null,
            keyStorageFlags: X509KeyStorageFlags.EphemeralKeySet);

        return (cert, rsa);
    }
}
