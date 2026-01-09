using Altinn.AccessMgmt.Core.Utils.Models;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;

namespace Altinn.AccessMgmt.Core.Services.Contracts
{
    public interface IErrorQueueService
    {
        /// <summary>
        /// Add an error to the queue
        /// </summary>
        /// <param name="error">the model to log</param>
        /// <param name="values">Audit values</param>
        /// <param name="cancellation">The <see cref="CancellationToken"/></param>
        /// <returns></returns>
        Task<bool> AddErrorQueue(ErrorQueue error, AuditValues values, CancellationToken cancellation);
    }
}
