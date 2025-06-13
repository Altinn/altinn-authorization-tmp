using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.Authorization.Api.Contracts.Consent;

namespace Altinn.AccessManagement.Api.Internal.Extensions
{
    /// <summary>
    /// Provides extension methods for transforming ConsentRight to ConsentRightExternal.
    /// </summary>
    public static class ConsentRightExtensions
    {
        /// <summary>
        /// Converts a <see cref="ConsentRight"/> object to a <see cref="ConsentRightExternal"/> object.
        /// </summary>
        /// <param name="core">The <see cref="ConsentRight"/> object to convert.</param>
        /// <returns>A <see cref="ConsentRightExternal"/> object representing the converted data.</returns>
        public static ConsentRightExternal ToConsentRightExternal(this ConsentRight core)
        {
            return new ConsentRightExternal
            {
                Action = core.Action,
                Resource = core.Resource.Select(static x => x.ToConsentResourceAttributeExternal()).ToList(),
                Metadata = core.Metadata
            };
        }
    }
}
