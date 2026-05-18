using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessManagement.Core.Configuration
{
    public class ConsentSettings
    {
        /// <summary>
        /// The number of consent events to retrieve per page when querying the consent events
        /// </summary>
        public int EventsPageSize { get; set; } = 100;
        
        /// <summary>
        /// The number of seconds to wait before considering a consent event as final. This setting is used to introduce a safety lag when processing consent events, ensuring that any recent changes are fully propagated and reducing the risk of inconsistencies. Adjusting this value can help balance the trade-off between data freshness and consistency.
        /// </summary>
        public int EventsSafetyLagSeconds { get; set; } = 5;
    }
}
