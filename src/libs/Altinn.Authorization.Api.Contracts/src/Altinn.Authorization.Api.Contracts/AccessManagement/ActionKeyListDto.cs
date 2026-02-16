using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Authorization.Api.Contracts.AccessManagement
{
    public class ActionKeyListDto
    {
        public IEnumerable<string> ActionKeys { get; set; }
    }
}
