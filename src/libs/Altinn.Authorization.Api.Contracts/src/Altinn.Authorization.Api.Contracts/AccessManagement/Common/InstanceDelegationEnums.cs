using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InstanceDelegationModeExternal
{
    Normal,
    ParallelSigning
}