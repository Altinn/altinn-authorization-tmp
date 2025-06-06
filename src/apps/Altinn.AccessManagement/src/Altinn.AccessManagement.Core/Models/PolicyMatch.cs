using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Enums;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// This model describes a single rule in a delegated policy
    /// </summary>
    public class PolicyMatch
    {
        /// <summary>
        /// Gets or sets the unique identifier for a specific party for which the requested rule in the policy applies
        /// </summary>
        public int OfferedByPartyId { get; set; }

        /// <summary>
        /// The type of FromUuid
        /// </summary>
        public UuidType FromUuidType { get; set; }

        /// <summary>
        /// The values of the FromUuid
        /// </summary>
        public Guid FromUuid { get; set; }

        /// <summary>
        /// Gets or sets resource match which uniquely identifies the resource this policy applies to.
        /// </summary>
        public List<AttributeMatch> Resource { get; set; }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for the coveredby id
        /// </summary>
        [Required]
        public List<AttributeMatch> CoveredBy { get; set; }
    }
}
