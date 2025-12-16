using System.Runtime.Serialization;

namespace Altinn.AccessManagement.Core.Models.Consent
{
    /// <summary>
    /// Defines the view mode of the consent portal for external users.
    /// </summary>
    public enum ConsentPortalViewMode
    {
        [EnumMember(Value = "undefined")]
        Undefined = 0,

        [EnumMember(Value = "hide")]
        Hide = 1,

        [EnumMember(Value = "show")]
        Show = 2,
    }
}
