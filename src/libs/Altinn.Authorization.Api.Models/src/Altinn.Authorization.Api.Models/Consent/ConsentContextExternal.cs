using Altinn.Authorization.Core.Models.Consent;

namespace Altinn.Authorization.Api.Models.Consent
{
    public class ConsentContextExternal
    {
        /// <summary>
        /// The context of the consent. This is a free text field that can be used to describe the context of the consent. Saved in the language presented to the user.
        /// </summary>
        public required string Context { get; set; }

        public required string Language { get; set; }

        /// <summary>
        /// The consent context of reasources. Containes the consent 
        /// </summary>
        public List<ResourceContextExternal> ConsentContextResources { get; set; } = [];

        public ConsentContext ToCore() => new Altinn.Authorization.Core.Models.Consent.ConsentContext
        {
            Language = Language,
            Context = Context,
            ConsentContextResources = [.. ConsentContextResources.Select(x => x.ToCore())]
        };

    }
}
