using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.Authorization.Api.Models.Consent;

namespace Altinn.AccessManagement.Api.Enterprise.Extensions
{
    /// <summary>
    /// Provides extension methods for transforming ConsentResourceAttribute to ConsentResourceAttributeExternal.
    /// </summary>
    public static class ConsentResourceAttributeExtensions
    {
        /// <summary>
        /// Maps a ConsentResourceAttribute to a ConsentResourceAttributeExternal.
        /// </summary>
        /// <param name="core">the ConsentResourceAttribute</param>
        /// <returns></returns>
        public static ConsentResourceAttributeExternal ToConsentResourceAttributeExternal(this ConsentResourceAttribute core)
        {
            return new ConsentResourceAttributeExternal
            {
                Type = core.Type,
                Value = core.Value
            };
        }
    }
}
