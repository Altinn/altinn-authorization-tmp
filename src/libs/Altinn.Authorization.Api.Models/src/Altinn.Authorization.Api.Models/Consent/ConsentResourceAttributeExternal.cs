using Altinn.Authorization.Core.Models.Consent;

namespace Altinn.Authorization.Api.Models.Consent
{
    /// <summary>
    /// A resurce attribute identifying part or whole resource
    /// </summary>
    public class ConsentResourceAttributeExternal
    {
        /// <summary>
        /// The type of resource attribute. is a urn
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// The value of the resource attribute
        /// </summary>
        public required string Value { get; set; }
        
        /// <summary>
        /// Map from external consent resource attribute to internal consent resource attribute
        /// </summary>
        public static ConsentResourceAttribute ToCore(ConsentResourceAttributeExternal external)
        {
            return new ConsentResourceAttribute
            {
                Type = external.Type,
                Value = external.Value
            };
        }
    }
}
