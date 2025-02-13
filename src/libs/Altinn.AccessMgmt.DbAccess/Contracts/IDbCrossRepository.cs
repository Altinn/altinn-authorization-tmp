namespace Altinn.AccessMgmt.DbAccess.Contracts;

/// <summary>
/// Represents a cross repository that extends the capabilities of an extended repository
/// by providing additional methods to retrieve related entities of types <typeparamref name="TA"/> and <typeparamref name="TB"/>.
/// </summary>
/// <typeparam name="T">The primary entity type.</typeparam>
/// <typeparam name="TExtended">
/// The extended entity type which includes additional or computed properties related to the primary entity.
/// </typeparam>
/// <typeparam name="TA">The type of the related entity set A.</typeparam>
/// <typeparam name="TB">The type of the related entity set B.</typeparam>
public interface IDbCrossRepository<T, TExtended, TA, TB> : IDbExtendedRepository<T, TExtended>
{
    /// <summary>
    /// Retrieves a collection of related entities of type <typeparamref name="TA"/> associated with the primary entity identified by the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier of the primary entity.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the collection of related entities of type <typeparamref name="TA"/>.
    /// </returns>
    Task<IEnumerable<TA>> GetA(Guid id);

    /// <summary>
    /// Retrieves a collection of related entities of type <typeparamref name="TB"/> associated with the primary entity identified by the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier of the primary entity.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the collection of related entities of type <typeparamref name="TB"/>.
    /// </returns>
    Task<IEnumerable<TB>> GetB(Guid id);
}