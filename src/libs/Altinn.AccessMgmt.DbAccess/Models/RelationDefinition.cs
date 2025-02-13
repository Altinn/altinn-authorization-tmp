namespace Altinn.AccessMgmt.DbAccess.Models;

public class RelationDefinition
{
    public Type Base { get; set; }
    public Type Ref { get; set; }
    public string BaseProperty { get; set; } //UserId
    public string RefProperty { get; set; } //Id
    public string ExtendedProperty { get; set; } //User
}
