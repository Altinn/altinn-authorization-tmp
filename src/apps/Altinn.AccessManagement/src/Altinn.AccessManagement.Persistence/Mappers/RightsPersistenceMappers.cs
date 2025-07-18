using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.AccessManagement.Persistence.Models;

namespace Altinn.AccessManagement.Persistence.Mappers;

public static class RightsPersistenceMappers
{
    public static Right ToCore(this RightEntity entity)
    {
        return new Right
        {
            RightKey = entity.RightKey,
            Resource = entity.Resource?.Select(r => r.ToCore()).ToList() ?? [],
            Action = entity.Action
        };
    }

    public static RightEntity ToEntity(this Right core)
    {
        return new RightEntity
        {
            RightKey = core.RightKey,
            Resource = core.Resource?.Select(r => r.ToEntity()).ToList() ?? [],
            Action = core.Action
        };
    }

    public static RightDelegationResult ToCore(this RightDelegationResultEntity entity)
    {
        return new RightDelegationResult
        {
            RightKey = entity.RightKey,
            Resource = entity.Resource?.Select(r => r.ToCore()).ToList() ?? [],
            Action = entity.Action,
            Status = entity.Status switch
            {
                "Delegated" => DelegationStatus.Delegated,
                "NotDelegated" => DelegationStatus.NotDelegated,
                _ => DelegationStatus.NotDelegated
            },
            Details = entity.Details?.Select(d => d.ToCore()).ToList() ?? []
        };
    }

    public static RightDelegationResultEntity ToEntity(this RightDelegationResult core)
    {
        return new RightDelegationResultEntity
        {
            RightKey = core.RightKey,
            Resource = core.Resource?.Select(r => r.ToEntity()).ToList() ?? [],
            Action = core.Action,
            Status = core.Status switch
            {
                DelegationStatus.Delegated => "Delegated",
                DelegationStatus.NotDelegated => "NotDelegated",
                _ => "NotDelegated"
            },
            Details = core.Details?.Select(d => d.ToEntity()).ToList() ?? []
        };
    }
}