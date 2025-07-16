using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Persistence.Models;

namespace Altinn.AccessManagement.Persistence.Mappers;

public static class AttributeMatchPersistenceMappers
{
    public static AttributeMatch ToCore(this AttributeMatchEntity entity)
    {
        return new AttributeMatch
        {
            Id = entity.AttributeId,
            Value = entity.AttributeValue,
            Type = entity.AttributeType switch
            {
                "Equals" => AttributeMatchType.Equals,
                "Contains" => AttributeMatchType.Contains,
                _ => AttributeMatchType.Equals
            }
        };
    }

    public static AttributeMatchEntity ToEntity(this AttributeMatch core)
    {
        return new AttributeMatchEntity
        {
            AttributeId = core.Id,
            AttributeValue = core.Value,
            AttributeType = core.Type switch
            {
                AttributeMatchType.Equals => "Equals",
                AttributeMatchType.Contains => "Contains",
                _ => "Equals"
            }
        };
    }
}