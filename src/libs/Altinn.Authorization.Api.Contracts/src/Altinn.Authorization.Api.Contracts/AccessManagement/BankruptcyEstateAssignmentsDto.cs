using System;
using System.Collections.Generic;
using System.Text;

namespace Altinn.Authorization.Api.Contracts.AccessManagement
{
    public class BankruptcyEstateAssignmentsDto
    {
        public Guid AssignmentId { get; set; }

        public Guid? PackageId { get; set; }
        
        public Guid FromId { get; set; }

        public Guid ToId { get; set; }

        public string PackageCode { get; set; }
    }
}
