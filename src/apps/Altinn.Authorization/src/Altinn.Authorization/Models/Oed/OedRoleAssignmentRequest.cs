namespace Altinn.Platform.Authorization.Models
{
    /// <summary>
    /// Model for requesting OED/Digitalt d�dsbo role assignments between two persons
    /// </summary>
    public class OedRoleAssignmentRequest
    {
        /// <summary>
        /// The person the OED/Digitalt d�dsbo role if provided from (the deceased)
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// The person the OED/Digitalt d�dsbo role if provided to
        /// </summary>
        public string To { get; set; }
    }
}
