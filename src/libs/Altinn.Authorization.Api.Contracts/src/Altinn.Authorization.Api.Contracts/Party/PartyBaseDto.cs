
namespace Altinn.Authorization.Api.Contracts.Party
{
    public class PartyBaseDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the party.
        /// </summary>
        public Guid PartyUuid { get; set; }

        /// <summary>
        /// Gets or sets the type of the PArty Uuid.
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity variant.
        /// </summary>
        public string EntityVariantType { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the parent party.
        /// </summary>
        public Guid? ParentPartyUuid { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who created the entity.
        /// </summary>
        public Guid? CreatedBy { get; set; }
    }
}

