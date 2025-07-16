using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.Authorization.Api.Contracts.AccessManagement.Common;
using Altinn.Authorization.Api.Contracts.AccessManagement.Rights;

namespace Altinn.AccessManagement.Mappers;

public static class RightsMappers
{
    public static RightsQueryExternal ToDto(this RightsQuery core)
    {
        return new RightsQueryExternal
        {
            From = core.From?.Select(f => f.ToDto()).ToList() ?? [],
            To = core.To?.Select(t => t.ToDto()).ToList() ?? [],
            Resource = core.Resource?.Select(r => r.ToDto()).ToList() ?? []
        };
    }

    public static RightsQuery ToCore(this RightsQueryExternal dto)
    {
        return new RightsQuery
        {
            From = dto.From?.Select(f => f.ToCore()).ToList() ?? [],
            To = dto.To?.Select(t => t.ToCore()).ToList() ?? [],
            Resource = dto.Resource?.Select(r => r.ToCore()).ToList() ?? []
        };
    }

    public static RightExternal ToDto(this Right core)
    {
        return new RightExternal
        {
            RightKey = core.RightKey,
            Resource = core.Resource?.Select(r => r.ToDto()).ToList() ?? [],
            Action = core.Action,
            RightSources = core.RightSources?.Select(rs => rs.ToDto()).ToList() ?? []
        };
    }

    public static Right ToCore(this RightExternal dto)
    {
        return new Right
        {
            RightKey = dto.RightKey,
            Resource = dto.Resource?.Select(r => r.ToCore()).ToList() ?? [],
            Action = dto.Action,
            RightSources = dto.RightSources?.Select(rs => rs.ToCore()).ToList() ?? []
        };
    }

    public static RightDelegationCheckResultExternal ToDto(this RightDelegationCheckResult core)
    {
        return new RightDelegationCheckResultExternal
        {
            RightKey = core.RightKey,
            Resource = core.Resource?.Select(r => r.ToDto()).ToList() ?? [],
            Action = core.Action,
            Status = core.Status switch
            {
                DelegationStatus.Delegated => DelegationStatusDto.Delegated,
                DelegationStatus.NotDelegated => DelegationStatusDto.NotDelegated,
                _ => DelegationStatusDto.NotDelegated
            },
            Details = core.Details?.Select(d => d.ToDto()).ToList() ?? []
        };
    }

    public static RightDelegationCheckResult ToCore(this RightDelegationCheckResultExternal dto)
    {
        return new RightDelegationCheckResult
        {
            RightKey = dto.RightKey,
            Resource = dto.Resource?.Select(r => r.ToCore()).ToList() ?? [],
            Action = dto.Action,
            Status = dto.Status switch
            {
                DelegationStatusDto.Delegated => DelegationStatus.Delegated,
                DelegationStatusDto.NotDelegated => DelegationStatus.NotDelegated,
                _ => DelegationStatus.NotDelegated
            },
            Details = dto.Details?.Select(d => d.ToCore()).ToList() ?? []
        };
    }

    public static RightsDelegationRequestExternal ToDto(this RightsDelegationRequest core)
    {
        return new RightsDelegationRequestExternal
        {
            To = core.To?.Select(t => t.ToDto()).ToList() ?? [],
            Rights = core.Rights?.Select(r => r.ToDto()).ToList() ?? []
        };
    }

    public static RightsDelegationRequest ToCore(this RightsDelegationRequestExternal dto)
    {
        return new RightsDelegationRequest
        {
            To = dto.To?.Select(t => t.ToCore()).ToList() ?? [],
            Rights = dto.Rights?.Select(r => r.ToCore()).ToList() ?? []
        };
    }

    public static RightsDelegationResponseExternal ToDto(this RightsDelegationResponse core)
    {
        return new RightsDelegationResponseExternal
        {
            To = core.To?.Select(t => t.ToDto()).ToList() ?? [],
            RightDelegationResults = core.RightDelegationResults?.Select(rdr => rdr.ToDto()).ToList() ?? []
        };
    }

    public static RightsDelegationResponse ToCore(this RightsDelegationResponseExternal dto)
    {
        return new RightsDelegationResponse
        {
            To = dto.To?.Select(t => t.ToCore()).ToList() ?? [],
            RightDelegationResults = dto.RightDelegationResults?.Select(rdr => rdr.ToCore()).ToList() ?? []
        };
    }
}