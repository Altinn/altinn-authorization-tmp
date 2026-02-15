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
            RequestId = request.Id,
            From = Convert(request.From),
            To = Convert(request.To),
            By = Convert(request.RequestedBy),
            Status = request.Status
        };
    }

    public static RequestDto Convert(RequestAssignmentPackage request)
    {
        return new RequestDto
        {
            RequestId = request.Id,
            From = Convert(request.Assignment.From),
            To = Convert(request.Assignment.To),
            By = Convert(request.RequestedBy),
            Status = request.Status
        };
    }

    public static RequestDto Convert(RequestAssignmentResource request)
    {
        return new RequestDto
        {
            RequestId = request.Id,
            From = Convert(request.Assignment.From),
            To = Convert(request.Assignment.To),
            By = Convert(request.RequestedBy),
            Status = request.Status
        };
    }
}
