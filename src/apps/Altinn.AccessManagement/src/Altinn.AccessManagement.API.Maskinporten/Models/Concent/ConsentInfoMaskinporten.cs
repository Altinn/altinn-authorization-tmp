namespace Altinn.AccessManagement.Api.Maskinporten.Models.Concent
{
    /// <summary>
    /// Represents the consent information for Maskinporten.
    /// </summary>
    public class ConsentInfoMaskinporten
    {
        Guid Id { get; set; }


        ConsentPartyUrn From { get; set; }

        /// <summary>
        /// Defines the party requesting consent.
        /// </summary>
        ConsentPartyUrn To { get; set; }

        DateTimeOffset Concented { get; set; }

        DateTimeOffset ValidTo { get; set; }
    }
}
