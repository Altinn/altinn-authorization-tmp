using Altinn.AccessManagement.Core.Models.Consent;

namespace Altinn.AccessManagement.Api.Enterprise.Models.Consent
{
    /// <summary>
    /// Represents a right in a consent.
    /// </summary>
    public class ConsentRightExternal2
    {
        /// <summary>
        /// The action in the consent. Read, write etc. Can be multiple but in most concents it is only one.
        /// </summary>
        public required List<string> Action { get; set; }

        /// <summary>
        /// The resource attribute that identifies the resource part of the right. Can be multiple but in most concents it is only one.
        /// </summary>
        public required List<ConsentResourceAttributeExternal2> Resource
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
        public static ConsentRightExternal2 FromCore(ConsentRight core)
        {
            return new ConsentRightExternal2
            {
                Action = core.Action,
                Resource = core.Resource.Select(ConsentResourceAttributeExternal2.FromCore).ToList(),
                MetaData = core.MetaData
            };
        }

        /// <summary>
        /// Maps from external consent right to internal consent right
        /// </summary>
        public static ConsentRight ToCore(ConsentRightExternal2 external)
        {
            return new ConsentRight
            {
                Action = external.Action,
                Resource = external.Resource.Select(ConsentResourceAttributeExternal2.ToCore).ToList(),
                MetaData = external.MetaData
            };
        }
    }
}
