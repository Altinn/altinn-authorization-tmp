using Altinn.Authorization.Api.Contracts.AccessManagement;

namespace Altinn.AccessMgmt.Core.Utils;

/// <inheritdoc/>
public partial class DtoMapper : IDtoMapper
{
    public static VariantPrivilegeDto ConvertToVariantPrivledgeDto(RoleVariantPrivilegeDto dto)
    {
        return new VariantPrivilegeDto()
        {
            Variant = dto.Variant,
            Packages = dto.Packages,
            Resources = dto.Resources,
        };
    }

    public static RolePrivilegeDto ConvertToRolePrivledgeDto(RoleVariantPrivilegeDto dto)
    {
        return new RolePrivilegeDto()
        {
            Role = dto.Role,
            Packages = dto.Packages,
            Resources = dto.Resources,
        };
    }
}
