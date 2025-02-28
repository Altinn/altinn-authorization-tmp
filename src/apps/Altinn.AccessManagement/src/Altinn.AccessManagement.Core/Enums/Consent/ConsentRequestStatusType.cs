using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace Altinn.AccessManagement.Core.Enums.Consent
{
    /// <summary>
    /// Enum for the status of a consent request
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ConsentRequestStatusType : int
    {
        [EnumMember(Value = "created")]
        [PgName("created")]
        Created = 0,

        [EnumMember(Value = "rejected")]
        [PgName("rejected")]
        Rejected = 1,

        [EnumMember(Value = "approved")]
        [PgName("approved")]
        Approved = 2
    }
}
