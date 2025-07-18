using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RightSourceTypeDto
{
    Role,
    Delegation,
    AccessList,
    SystemUser,
    Maskinporten
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RightSourceTypeExternal
{
    Role,
    AccessGroup,
    AppDelegation,
    ResourceRegistryDelegation,
    MainUnit,
    InheritedFromMainUnit,
    SystemUser,
    Maskinporten
}