namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// Represents the base data transfer object for a party, including its unique identifier, type, and display name.
    /// </summary>
    public class PartyBaseDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PartyBaseDto"/> class with the specified unique identifier,
        /// type, and display name.
        /// </summary>
        /// <param name="partyUuid">The unique identifier for the party.</param>
        /// <param name="type">The type of the party, which categorizes the party entity.</param>
        /// <param name="displayName">The display name of the party, used for user-friendly identification.</param>
        public PartyBaseDto(Guid partyUuid, string type, string displayName)
        {
            PartyUuid = partyUuid;
            Type = type;
            DisplayName = displayName;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the party.
        /// </summary>
        public Guid PartyUuid { get; set; }

        /// <summary>
        /// Gets or sets the type of the UUID.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who created the entity.
        /// </summary>
        public Guid? CreatedBy { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }

        /*
        public PartyBaseDto Map()
        {
            bool success = Type.TryParseFromMemberName(out UuidType uuidType);

            return new PartyBaseDto
            {
                PartyUuid = PartyUuid,
                Type = success ? uuidType : UuidType.NotSpecified,
                DisplayName = DisplayName
            };
        }
        */
    }
}
