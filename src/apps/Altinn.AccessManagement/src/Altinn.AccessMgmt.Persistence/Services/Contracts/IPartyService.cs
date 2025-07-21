using Altinn.AccessManagement.Core.Models.Party;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.ProblemDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessMgmt.Persistence.Services.Contracts
{
    public interface IPartyService
    {
        public Task<Result<AddPartyResult>> AddParty(PartyBaseInternal request, ChangeRequestOptions options, CancellationToken cancellationToken = default);
    }
}
