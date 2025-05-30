namespace Altinn.Authorization.Core.Models.Consent
{
    /// <summary>
    /// Represents a dictionary of metadata.
    /// </summary>
    public class MetadataDictionary
       : Dictionary<string, string>
    {
        public MetadataDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
