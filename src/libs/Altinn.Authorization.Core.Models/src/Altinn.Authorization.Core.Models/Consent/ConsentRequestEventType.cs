using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace Altinn.Authorization.Core.Models.Consent
{
    /// <summary>
    /// Enum for the status of a consent request
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ConsentRequestEventType
    {
        [EnumMember(Value = "created")]
        [PgName("created")]
        Created = 0,

        [EnumMember(Value = "rejected")]
        [PgName("rejected")]
        Rejected = 1,

        [EnumMember(Value = "accepted")]
        [PgName("accepted")]
        Accepted = 2,

        [EnumMember(Value = "revoked")]
        [PgName("revoked")]
        Revoked = 3,

        [EnumMember(Value = "deleted")]
        [PgName("deleted")]
        Deleted = 4
    }
}
