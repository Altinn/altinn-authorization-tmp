using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Authorization.Core.Models.Consent
{
    public class ConsentTemplateTypeText
    {
        /// <summary>
        /// Texts used in consent for organization
        /// </summary>
        public Dictionary<string, string> Org { get; set; }

        /// <summary>
        /// Texts used in consent for person
        /// </summary>
        public Dictionary<string, string> Person { get; set; }
    }
}
