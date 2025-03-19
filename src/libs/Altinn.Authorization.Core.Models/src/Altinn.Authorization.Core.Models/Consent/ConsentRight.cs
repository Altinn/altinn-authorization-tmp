using System.Text.Json.Serialization;

namespace Altinn.Authorization.Core.Models.Consent
{
    /// <summary>
    /// Represents a right in a consent.
    /// </summary>
    public class ConsentRight
    {
        /// <summary>
        /// The action in the consent. Read, write etc. Can be multiple but in most concents it is only one.
        /// </summary>
        public required List<string> Action { get; set; }

        /// <summary>
        /// The resource attribute that identifies the resource part of the right. Can be multiple but in most concents it is only one.
        /// </summary>
        public required List<ConsentResourceAttribute> Resource
        {
            get; set;
        }

        /// <summary>
        /// The metadata for the right. Can be multiple but in most concents it is only one.   
        /// Keys are case insensitive.
        /// </summary>
        public Dictionary<string, string> MetaData { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);    

        public void SetMetaData(IDictionary<string, string>? keyValuePairs)
        {
            MetaData.Clear();
            if (keyValuePairs == null)
            {
                return;
            }

            foreach (var item in keyValuePairs)
            {
                MetaData.Add(item.Key, item.Value);
            }
        }
    }
}
