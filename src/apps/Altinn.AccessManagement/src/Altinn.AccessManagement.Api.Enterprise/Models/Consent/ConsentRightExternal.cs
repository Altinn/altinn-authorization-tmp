using Altinn.AccessManagement.Core.Models.Consent;

namespace Altinn.AccessManagement.Api.Enterprise.Models.Consent
{
    /// <summary>
    /// Represents a right in a consent.
    /// </summary>
    public class ConsentRightExternal
    {
        /// <summary>
        /// The action in the consent. Read, write etc. Can be multiple but in most concents it is only one.
        /// </summary>
        public required List<string> Action { get; set; }

        /// <summary>
        /// The resource attribute that identifies the resource part of the right. Can be multiple but in most concents it is only one.
        /// </summary>
        public required List<ConsentResourceAttributeExternal> Resource
        {
            get; set;
        }

        /// <summary>
        /// Metadata for consent resource right not required
        /// </summary>
        public Dictionary<string, string>? MetaData { get; set; }

        /// <summary>
        /// Maps from internal consent right to external consent right
        /// </summary>
        public static ConsentRightExternal FromCore(ConsentRight core)
        {
            return new ConsentRightExternal
            {
                Action = core.Action,
                Resource = core.Resource.Select(ConsentResourceAttributeExternal.FromCore).ToList(),
                MetaData = core.MetaData
            };
        }
    }
}
