using Altinn.AccessMgmt.PersistenceEF.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Base
{
    public class BaseA2ClientRole
    {
        private Guid _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseA2ClientRole"/> class.
        /// </summary>
        public BaseA2ClientRole()
        {
            Id = Guid.CreateVersion7();
        }

        /// <summary>
        /// Identity
        /// </summary>
        public Guid Id
        {
            get => _id;
            set
            {
                if (!value.IsVersion7Uuid())
                {
                    throw new ArgumentException("Id must be a version 7 UUID", nameof(value));
                }

                _id = value;
            }
        }
    }
}
