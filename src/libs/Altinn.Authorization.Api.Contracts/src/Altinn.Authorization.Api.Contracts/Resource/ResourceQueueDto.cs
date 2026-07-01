using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Authorization.Api.Contracts.Resource
{
    public class ResourceQueueDto
    {
        public long Id { get; set; }

        public string ResourceIdentifier { get; set; }
    }
}
