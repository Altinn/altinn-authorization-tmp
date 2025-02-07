namespace Altinn.AccessMgmt.DbAccess;

public class DbAccessConfig
{
    public string ConnectionStringAdmin { get; set; }
    public string ConnectionString { get; set; }
    public string BaseSchema { get; set; } = "dbo";
    public bool MigrationEnabled { get; set; }
    public string MigrationKey { get; set; }
    public List<string> JsonIngestLanguages { get; set; }
    public string JsonBasePath { get; set; } = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Ingest/JsonData/");
    public Dictionary<string, bool> JsonIngestEnabled { get; set; } = new Dictionary<string, bool>();
    public bool MockEnabled { get; set; }
    public Dictionary<string, bool> MockRun { get; set; } = new Dictionary<string, bool>();
}
