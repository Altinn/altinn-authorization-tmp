using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;

namespace Altinn.AccessMgmt.Core.Utils;

/// <inheritdoc/>
public partial class DtoMapper : IDtoMapper
{
    public static ClientDto ConvertToClientDto(Delegation delegation)
    {
        return new()
        {
            Client = delegation.From
            Access = delegation.To.
        };
    }
}
