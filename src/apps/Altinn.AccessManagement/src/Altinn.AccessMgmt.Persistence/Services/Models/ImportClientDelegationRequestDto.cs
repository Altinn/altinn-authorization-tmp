using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessMgmt.Persistence.Services.Models
{
    public class ImportClientDelegationRequestDto
    {
        /// <summary>
        /// Client party uuid
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Agent party uuid
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// Agent role (Facilitator -> Agent)
        /// e.g Agent
        /// </summary>
        public string AgentRole { get; set; } = string.Empty;

        /// <summary>
        /// The user that performed the delegation
        /// </summary>
        public Guid? Delegater { get; set; }

        /// <summary>
        /// The Service unit that performed the delegation
        /// </summary>
        public Guid? Facilitator { get; set; }

        /// <summary>
        /// The date and time when the delegation was performed
        /// </summary>
        public DateTimeOffset DelegatedDateTimeOffset { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Packages to be delegated to Agent
        /// </summary>
        public List<CreateSystemDelegationRolePackageDto> RolePackages { get; set; } = [];
    }
}
