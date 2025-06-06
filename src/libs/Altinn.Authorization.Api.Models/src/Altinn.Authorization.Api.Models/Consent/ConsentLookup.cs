namespace Altinn.Authorization.Api.Models.Consent
{
    /// <summary>
    /// Model for looking up a consent.
    /// </summary>
    public class ConsentLookup
    {
        /// <summary>
        /// Defines the Id of the consent.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Defines the party that has approved a consent request.
        /// </summary>
        public required ConsentPartyUrnExternal From { get; set; }

        /// <summary>
        /// Defines the party receiving consent.
        /// </summary>
        public required ConsentPartyUrnExternal To { get; set; }
    }
}
