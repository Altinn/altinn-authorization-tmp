using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace Altinn.AccessMgmt.TestUtils.Utils;

public static class TestTokenGenerator
{
    public const string Issuer = "UnitTestIssuer";

    public const string Audience = "altinn.no";

    public static readonly RSA RootCertificateKey = RSA.Create(2048);

    public static readonly ClaimsIdentity ClaimsIdentityPerson = new("person");

    public static readonly ClaimsIdentity ClaimsIdentityOrganization = new("organization");

    public static readonly ClaimsIdentity ClaimsIdentityApp = new("app");

    private static readonly JwtSecurityTokenHandler _tokenHandler = new();

    private static readonly Lazy<X509Certificate2> _signingCert =
        new(() =>
        {
            var req = new CertificateRequest(
                "CN=TestSigning",
                RootCertificateKey,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );

            req.CertificateExtensions.Add(
                new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

            var temp = req.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddYears(1));

            return X509CertificateLoader.LoadPkcs12(
                temp.Export(X509ContentType.Pfx),
                password: null,
                keyStorageFlags: X509KeyStorageFlags.EphemeralKeySet);
        });

    public static X509Certificate2 SigningCert => _signingCert.Value;

    public static X509SecurityKey SigningKey => new(SigningCert);

    public static string CreateToken(ClaimsIdentity claimsIdentity, params Action<List<Claim>>[] configureClaims)
    {
        var claims = new List<Claim>();
        foreach (var configure in configureClaims)
        {
            configure(claims);
        }

        var creds = new SigningCredentials(new RsaSecurityKey(RootCertificateKey), SecurityAlgorithms.RsaSha256);
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            notBefore: now,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: creds);

        return _tokenHandler.WriteToken(token);
    }
}
