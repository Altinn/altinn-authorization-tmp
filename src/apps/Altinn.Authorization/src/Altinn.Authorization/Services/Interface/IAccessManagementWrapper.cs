using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Altinn.Platform.Authorization.Models;
using Altinn.Platform.Authorization.Models.AccessManagement;

namespace Altinn.Platform.Authorization.Services.Interface
{
    /// <summary>
    /// The service used to map internal delegation change to delegation change events and push them to the event queue.
    /// </summary>
    public interface IAccessManagementWrapper
    {
        /// <summary>
        /// Endpoint to find all delegation changes for a given user, reportee and app/resource context
        /// </summary>
        /// <returns>Input parameter to the request</returns>
        public Task<IEnumerable<DelegationChangeExternal>> GetAllDelegationChanges(DelegationChangeInput input);

        /// <summary>
        /// Endpoint to find all delegation changes for a given user, reportee and app/resource context
        /// </summary>
        /// <returns>optional funvation pattern for modifying the request sent to Access Management API</returns>
        public Task<IEnumerable<DelegationChangeExternal>> GetAllDelegationChanges(params Action<DelegationChangeInput>[] actions);
    }
}
