namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// IChangeResult
/// </summary>
public interface IChangeResult
{
    /// <summary>
    /// Links
    /// </summary>
    ResultLinks Links { get; set; }

    /// <summary>
    /// Page
    /// </summary>
    ResultPage Page { get; set; }
}
