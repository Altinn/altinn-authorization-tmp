using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessMgmt.Core.Models
{
    public class ActionAccess
    {
        public string ActionKey { get; set; }

        public IEnumerable<string> AccessorUrns { get; set; }
    }
}
