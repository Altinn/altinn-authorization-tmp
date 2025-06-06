namespace Altinn.Authorization.Core.Models.Consent
{
    /// <summary>
    /// Wrapper class for ConsentRequest to be used in the consent request service to return if the consent request already existed or not.
    /// </summary>
    public class ConsentRequestDetailsWrapper
    {
        public required ConsentRequestDetails ConsentRequest { get; set; }

        public bool AlreadyExisted { get; set; }
    }
}
