namespace Altinn.Authorization.AccessPackages.Repo.Ingest;

/// <summary>
/// JsonIngestConfig
/// </summary>
public class JsonIngestConfig
{
    /// <summary>
    /// Basepath to data files
    /// </summary>
    public string BasePath { get; set; }

    /// <summary>
    /// List of used languagecodes
    /// </summary>
    public List<string> Languages { get; set; } = ["nno", "eng"];
}
