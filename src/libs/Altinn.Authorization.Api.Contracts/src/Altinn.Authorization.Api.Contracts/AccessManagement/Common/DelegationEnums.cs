using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DelegationStatusDto
{
    Active,
    Revoked,
    Expired
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DelegableStatusExternal
{
    NotDelegable,
    Delegable
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DelegationStatusExternal
{
    NotDelegated,
    Delegated
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RevokeStatusExternal
{
    NotRevoked,
    Revoked
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DelegationChangeTypeExternal
{
    Grant,
    Revoke
}