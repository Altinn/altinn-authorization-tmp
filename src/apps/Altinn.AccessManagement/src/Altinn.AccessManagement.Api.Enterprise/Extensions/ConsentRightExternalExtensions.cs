using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.Authorization.Api.Contracts.Consent;

namespace Altinn.AccessManagement.Api.Enterprise.Extensions
{
    /// <summary>
    /// Provides extension methods for transforming ConsentRight to ConsentRightExternal.
    /// </summary>
    public static class ConsentRightExternalExtensions
    {
        /// <summary>
        /// Converts a <see cref="ConsentRightExternal"/> object to a <see cref="ConsentRight"/> object.
        /// </summary>
        /// <param name="core">The <see cref="ConsentRightExternal"/> object to convert.</param>
        /// <returns>A <see cref="ConsentRight"/> object representing the converted data.</returns>
        public static ConsentRight ToConsentRightExternal(this ConsentRightExternal external)
        {
            ConsentRight consentRight = new ConsentRight
            {
                Action = external.Action,
                Resource = external.Resource.Select(static x => x.ToConsentResourceAttribute()).ToList()
            };

            if (external.Metadata != null)
            {
                consentRight.Metadata = new MetadataDictionary();
                foreach (var item in external.Metadata)
                {
                    consentRight.Metadata.Add(item.Key, item.Value);
                }
            }

            return consentRight;
        }
    }
}
