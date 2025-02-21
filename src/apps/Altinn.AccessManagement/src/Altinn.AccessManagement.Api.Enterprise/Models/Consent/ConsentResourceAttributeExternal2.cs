using Altinn.AccessManagement.Core.Models.Consent;

namespace Altinn.AccessManagement.Api.Enterprise.Models.Consent
{
    /// <summary>
    /// A resurce attribute identifying part or whole resource
    /// </summary>
    public class ConsentResourceAttributeExternal2
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
        /// Map from internal consent resource attribute to external consent resource attribute
        /// </summary>
        public static ConsentResourceAttributeExternal2 FromCore(ConsentResourceAttribute core)
        {
            return new ConsentResourceAttributeExternal2
            {
                Type = core.Type,
                Value = core.Value
            };
        }

        /// <summary>
        /// Map from external consent resource attribute to internal consent resource attribute
        /// </summary>
        public static ConsentResourceAttribute ToCore(ConsentResourceAttributeExternal2 external)
        {
            return new ConsentResourceAttribute
            {
                Type = external.Type,
                Value = external.Value
            };
        }
    }
}
