using Altinn.AccessManagement.Core.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement.Metadata;

namespace Altinn.AccessManagement.Api.Enduser.Mappers;

public static class PagingMappers
{
    public static Paging ToCore(this PagingInput dto)
    {
        return new Paging
        {
            Size = dto.Size,
            Page = dto.Page,
            SortBy = dto.SortBy,
            SortOrder = dto.SortOrder
        };
    }

    public static PagingInput ToDto(this Paging core)
    {
        return new PagingInput
        {
            Size = core.Size,
            Page = core.Page,
            SortBy = core.SortBy,
            SortOrder = core.SortOrder
        };
    }
}