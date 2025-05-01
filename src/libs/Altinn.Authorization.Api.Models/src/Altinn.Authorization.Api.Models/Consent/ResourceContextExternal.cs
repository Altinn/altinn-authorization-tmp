using Altinn.Authorization.Core.Models.Consent;
using Microsoft.Extensions.Hosting;

namespace Altinn.Authorization.Api.Models.Consent
{
    public class ResourceContextExternal
    {
        /// <summary>
        /// The resource id of the consent
        /// </summary>
        public required string ResourceId { get; set; }

        /// <summary>
        /// langauge used of the resource context when presented the context text
        /// </summary>
        public required string Language { get; set; }

        /// <summary>
        /// The resource context of the consent
        /// </summary>
        public required string Context { get; set; }

        public ResourceContext ToCore()
        {
            Guid guid = Guid.CreateVersion7();
            return new ResourceContext
            {
                ResourceId = ResourceId,
                Language = Language,
                Context = Context
            };
        }
    }
}
