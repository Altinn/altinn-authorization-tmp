namespace Altinn.AccessManagement.Api.Enduser.Models
{
    /// <summary>
    /// Lokkup Request query
    /// </summary>
    public class EntityLookupRequestDto
    {
        /// <summary>
        /// Gets or sets the type of the lookup value "OrganizationIdentifier" or "PersonIdentifier"
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value to lookup represented as a string.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the name associated with the lookup value only required when lookup is "PersonIdentifier".
        /// </summary>
        public string Name { get;set; }


    }
}
