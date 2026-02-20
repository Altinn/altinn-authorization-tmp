using Altinn.AccessMgmt.PersistenceEF.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessMgmt.PersistenceEF.Models
{
    public class RightImportProgress : BaseRightImportProgress
    {
        /// <summary>
        /// The associated delegationChangeId (app/resource/instance)
        /// </summary>
        public long DelegationChangeId { get; set; }

        /// <summary>
        /// The origin of the change (app/resource/instance)
        /// </summary>
        public string OriginType { get; set; }
    }
}
