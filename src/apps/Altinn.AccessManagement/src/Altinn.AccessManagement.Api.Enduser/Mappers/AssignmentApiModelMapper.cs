using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessMgmt.Core.Models;

namespace Altinn.AccessManagement.Api.Enduser.Mappers;

/// <summary>
/// Maps between <see cref="AssignmentApiModel"/> and <see cref="Assignment"/>.
/// </summary>
public class AssignmentApiModelMapper : IMapper<AssignmentApiModel, Assignment>
{
    /// <inheritdoc/>
    public AssignmentApiModel Map(Assignment from)
    {
        return new()
        {
            Id = from.Id,
            FromId = from.FromId,
            ToId = from.ToId,
            RoleId = from.RoleId,
        };
    }
}
