using System.Text;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;

namespace Altinn.AccessMgmt.Core.Utils;

/// <summary>
/// The DtoMapper is a partial class for converting database models and dto models
/// Create a new file for the diffrent areas
/// </summary>
public partial class DtoMapper : IDtoMapper
{
    public static RequestDto Convert(RequestAssignment request)
    {
        return new RequestDto
        {
            Id = request.Id,
            From = ConvertToPartyEntityDto(request.From),
            To = ConvertToPartyEntityDto(request.To),
            Status = request.Status
        };
    }

    public static RequestDto Convert(RequestAssignmentPackage request)
    {
        return new RequestDto
        {
            Id = request.Id,
            From = ConvertToPartyEntityDto(request.Assignment.From),
            To = ConvertToPartyEntityDto(request.Assignment.To),
            Status = request.Status
        };
    }

    public static RequestDto Convert(RequestAssignmentResource request)
    {
        return new RequestDto
        {
            Id = request.Id,
            From = ConvertToPartyEntityDto(request.Assignment.From),
            To = ConvertToPartyEntityDto(request.Assignment.To),
            Status = request.Status
        };
    }

    public static PartyEntityDto ConvertToPartyEntityDto(Entity entity)
    {
        return new PartyEntityDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type?.ToString(),
            SubType = entity.Variant?.ToString(),
            OrganizationIdentifier = entity.OrganizationIdentifier?.ToString(),
            PersonIdentifier = entity.PersonIdentifier?.ToString()
        };
    }
}
