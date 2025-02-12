namespace Altinn.AccessManagement.Api.Maskinporten.Models.Concent
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

        public Dictionary<string, string>? MetaData { get; set; }
    }
}
