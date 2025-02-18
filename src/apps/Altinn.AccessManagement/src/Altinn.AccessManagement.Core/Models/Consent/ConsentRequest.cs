namespace Altinn.AccessManagement.Core.Models.Consent
{
    /// <summary>
    /// Represents a consent request.
    /// </summary>
    public class ConsentRequest
    {
        public required Guid Id { get; set; }

        public required string Name { get; set; }
    }
}
