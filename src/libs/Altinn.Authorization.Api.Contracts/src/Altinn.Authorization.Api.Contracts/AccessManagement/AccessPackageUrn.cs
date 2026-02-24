using System.Globalization;
using Altinn.Urn;

namespace Altinn.Authorization.Api.Contracts.Consent
{
    /// <summary>
    /// A unique reference to a party in the form of an URN.
    /// </summary>
    [KeyValueUrn]
    public abstract partial record AccessPackageUrn
    {
        /// <summary>
        /// Try to get the urn as a party uuid.
        /// </summary>
        /// <param name="packageId">The resulting party uuid.</param>
        /// <returns><see langword="true"/> if this party reference is a party uuid, otherwise <see langword="false"/>.</returns>
        [UrnKey("altinn:accesspackage", Canonical = true)]
        public partial bool IsAccessPackage(out string packageId);

        // Manually overridden to disallow negative party ids
        private static bool TryParseAccessPackage(ReadOnlySpan<char> segment, IFormatProvider? provider, out string value)
        {
            value = new(segment);
            return true;
        }
    }
}
