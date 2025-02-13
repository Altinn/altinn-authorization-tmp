using System.Reflection;

namespace Altinn.AccessMgmt.DbAccess.Models;

public class ColumnDefinition
{
    public PropertyInfo Property { get; set; }
    public string Name { get; set; }
    public bool IsNullable { get; set; }
    public string? DefaultValue { get; set; }
    public int? Length { get; set; }
}

public class PKDefinition
{
    public List<PropertyInfo> Properties { get; set; }
    public Dictionary<string, Type> SimpleProperties { get; set; }
}
