using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions;

/// <summary>
/// Read helpers for entity state.
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    /// Tells whether the entity is a person that has died.
    /// </summary>
    /// <remarks>
    /// Persons imported from Altinn 2 can carry <see cref="DateOnly.MinValue"/> as a placeholder
    /// for "not dead", so only a later date of death counts. The party sync applies the same rule.
    /// </remarks>
    public static bool IsDeceasedPerson(this BaseEntity entity)
    {
        return entity.TypeId == EntityTypeConstants.Person.Id
            && entity.DateOfDeath is { } dateOfDeath
            && dateOfDeath > DateOnly.MinValue;
    }
}
