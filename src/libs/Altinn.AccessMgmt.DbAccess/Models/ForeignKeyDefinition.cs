using Altinn.AccessMgmt.DbAccess.Helpers;

namespace Altinn.AccessMgmt.DbAccess.Models;

public class ForeignKeyDefinition
{
    public string Name { get; set; }

    public Type Base { get; set; }
    public Type Ref { get; set; }
    public string BaseProperty { get; set; } //UserId
    public string RefProperty { get; set; } //Id
    public string ExtendedProperty { get; set; } //User

    public List<GenericFilter> Filters { get; set; } // Check that Value Property exists ...

    public bool IsList { get; set; }
    public bool IsOptional { get; set; }
    public bool UseCascadeDelete { get; set; }
}
