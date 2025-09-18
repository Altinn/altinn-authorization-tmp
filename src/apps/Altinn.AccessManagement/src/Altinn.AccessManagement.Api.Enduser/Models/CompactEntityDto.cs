namespace Altinn.AccessManagement.Api.Enduser.Models
{
    public class CompactEntityDto
    {
        public Guid id { get; set; }

        public int PartyId { get; set; }

        public int UserId { get; set; }

        public string OrganizationIdentifier { get; set; }

        public string PersonIdentifier { get; set; }
    }
}
