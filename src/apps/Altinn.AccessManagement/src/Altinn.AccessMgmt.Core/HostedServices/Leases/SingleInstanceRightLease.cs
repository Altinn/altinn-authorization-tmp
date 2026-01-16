using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessMgmt.Core.HostedServices.Leases
{
    internal class SingleInstanceRightLease
    {
        /// <summary>
        /// The URL of the next page of instance delegation data.
        /// </summary>
        public string SingleInstanceRightStreamNextPageLink { get; set; }
    }
}
