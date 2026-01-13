using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;

namespace Altinn.AccessMgmt.Core.Utils;

/// <inheritdoc/>
public partial class DtoMapper : IDtoMapper
{
    public static DelegationDto ConvertToDelegationDto(Delegation model, Guid packageId)
    {
        return new DelegationDto()
        {
            Id = model.Id,
            ToId = model.ToId,
            FromId = model.FromId,
            ViaId = model.FacilitatorId,
            PackageId = packageId
        };
    }

    public static DelegationDto ConvertToDelegationDto(ConnectionQueryExtendedRecord model, Guid packageId)
    {
        return new DelegationDto()
        {
            Id = (Guid)model.DelegationId,
            ToId = model.ToId,
            FromId = model.FromId,
            ViaId = (Guid)model.ViaId,
            PackageId = packageId
        };
    }
}
