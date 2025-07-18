using Altinn.AccessManagement.Core.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement.Connection;
using Altinn.Authorization.Api.Contracts.AccessManagement.Metadata;

namespace Altinn.AccessManagement.Api.Enduser.Mappers;

/// <summary>
/// Extension methods for fluent mapping between layers
/// </summary>
public static class MappingExtensions
{
    #region Connection Extensions

    // API → Core
    public static Connection ToCore(this ConnectionInput dto)
        => ConnectionMappers.ToCore(dto);

    // Core → API
    public static ConnectionInput ToDto(this Connection core)
        => ConnectionMappers.ToDto(core);

    #endregion

    #region Paging Extensions

    // API → Core
    public static Paging ToCore(this PagingInput dto)
        => PagingMappers.ToCore(dto);

    // Core → API
    public static PagingInput ToDto(this Paging core)
        => PagingMappers.ToDto(core);

    #endregion
}