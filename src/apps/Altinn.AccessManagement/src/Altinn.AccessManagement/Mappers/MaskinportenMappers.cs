using Altinn.AccessManagement.Core.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement.MaskinportenDelegation;

namespace Altinn.AccessManagement.Mappers;

public static class MaskinportenMappers
{
    public static MPDelegationExternal ToDto(this MPDelegation core)
    {
        return new MPDelegationExternal
        {
            SupplierOrg = core.SupplierOrg,
            ConsumerOrg = core.ConsumerOrg,
            Scope = core.Scope,
            Created = core.Created,
            PerformedBy = core.PerformedBy
        };
    }

    public static MPDelegation ToCore(this MPDelegationExternal dto)
    {
        return new MPDelegation
        {
            SupplierOrg = dto.SupplierOrg,
            ConsumerOrg = dto.ConsumerOrg,
            Scope = dto.Scope,
            Created = dto.Created,
            PerformedBy = dto.PerformedBy
        };
    }

    public static MaskinportenSchemaDelegationExternal ToDto(this MaskinportenSchemaDelegation core)
    {
        return new MaskinportenSchemaDelegationExternal
        {
            SupplierOrg = core.SupplierOrg,
            ConsumerOrg = core.ConsumerOrg,
            Scope = core.Scope,
            Created = core.Created,
            PerformedBy = core.PerformedBy
        };
    }

    public static MaskinportenSchemaDelegation ToCore(this MaskinportenSchemaDelegationExternal dto)
    {
        return new MaskinportenSchemaDelegation
        {
            SupplierOrg = dto.SupplierOrg,
            ConsumerOrg = dto.ConsumerOrg,
            Scope = dto.Scope,
            Created = dto.Created,
            PerformedBy = dto.PerformedBy
        };
    }
}