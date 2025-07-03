using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

using Microsoft.IdentityModel.Tokens;

namespace Altinn.AccessManagement.Tests.Utils
{
    /// <summary>
    /// Represents a mechanism for creating JSON Web tokens for use in integration tests.
    /// </summary>
    public static class JwtTokenMock
    {
        /// <summary>
        /// Generates a token with a self signed certificate included in the integration test project.
        /// </summary>
        /// <param name="principal">The claims principal to include in the token.</param>
        /// <param name="tokenExpiry">How long the token should be valid for.</param>
        /// <param name="issuer">The URL of the token issuer</param>
        /// <returns>A new token.</returns>
        public static string GenerateToken(ClaimsPrincipal principal, TimeSpan tokenExpiry, string issuer = "UnitTest")
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(principal.Identity),
                Expires = DateTime.UtcNow.AddSeconds(tokenExpiry.TotalSeconds),
                SigningCredentials = GetSigningCredentials(issuer),
                Audience = "altinn.no",
                Issuer = issuer
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string serializedToken = tokenHandler.WriteToken(token);

            return serializedToken;
        }

        private static X509SigningCredentials GetSigningCredentials(string issuer)
        {
            string certPath = "selfSignedTestCertificate.pfx";
            string certPassword = "qwer1234"; // Use config or env var in real app

            if (!issuer.Equals("sbl.authorization") && !issuer.Equals("www.altinn.no") && !issuer.Equals("UnitTest"))
            {
                certPath = $"{issuer}-org.pfx";
            }

            var certificates = X509CertificateLoader.LoadPkcs12CollectionFromFile(certPath, certPassword);
            var cert = certificates.FirstOrDefault(c => c.HasPrivateKey);

            if (cert is null)
            {
                throw new Exception($"No valid certificate with private key found in {certPath}");
            }

            return new X509SigningCredentials(cert, SecurityAlgorithms.RsaSha256);
        }
    }
}
