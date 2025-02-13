namespace Altinn.AccessMgmt.DbAccess.Models;

public class ConstraintDefinition
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public List<string> Columns { get; set; }
    public bool IsUnique { get; set; }
}
