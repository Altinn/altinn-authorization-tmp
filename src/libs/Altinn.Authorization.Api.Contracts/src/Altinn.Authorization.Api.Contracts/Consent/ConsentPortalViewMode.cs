using System.Runtime.Serialization;

namespace Altinn.Authorization.Api.Contracts.Consent
{
    /// <summary>
    /// Defines the view mode of the consent portal for external users.
    /// </summary>
    public enum ConsentPortalViewMode
    {
        [EnumMember(Value = "hide")]
        Hide = 0,

        [EnumMember(Value = "show")]
        Show = 1,
    }
}
