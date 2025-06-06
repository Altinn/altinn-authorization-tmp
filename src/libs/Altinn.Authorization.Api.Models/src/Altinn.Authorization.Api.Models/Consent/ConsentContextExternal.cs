using Altinn.Authorization.Core.Models.Consent;

namespace Altinn.Authorization.Api.Models.Consent
{
    public class ConsentContextExternal
    {
        public required string Language { get; set; }

        public ConsentContext ToCore() => new Altinn.Authorization.Core.Models.Consent.ConsentContext
        {
            Language = Language,
        };
    }
}
