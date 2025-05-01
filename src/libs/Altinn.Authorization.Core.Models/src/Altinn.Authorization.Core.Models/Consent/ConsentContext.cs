namespace Altinn.Authorization.Core.Models.Consent
{
    public class ConsentContext
    {
        /// <summary>
        /// The language used when consenting
        /// </summary>
        public required string Language { get; set; }

        /// <summary>
        /// The context of the consent. This is a free text field that can be used to describe the context of the consent. Saved in the language presented to the user.
        /// </summary>
        public required string Context { get; set; }

        /// <summary>
        /// The consent context of reasources. Containes the consent 
        /// </summary>
        public List<ResourceContext> ConsentContextResources { get; set; } = new List<ResourceContext>();

    }
}
