using System.ComponentModel.DataAnnotations;

namespace Altinn.AccessMgmt.DbAccess.Models;

public class DbAccessConfig
{
    public string DatabaseType { get; set; } = "Postgres";
    public string ConnectionString { get; set; } = string.Empty;
}
