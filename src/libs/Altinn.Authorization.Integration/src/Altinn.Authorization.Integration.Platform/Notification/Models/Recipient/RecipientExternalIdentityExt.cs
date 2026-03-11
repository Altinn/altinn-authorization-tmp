using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Notifications.Models.Recipient;

/// <summary>
/// Represents a notification recipient identified by an external identity.
/// </summary>
public class RecipientExternalIdentityExt : RecipientBaseExt
{
    /// <summary>
    /// The external identity of the recipient in URN format.
    /// </summary>
    /// <seealso cref="ExternalIdentityUrn"/>
    [Required]
    [JsonPropertyName("externalIdentity")]
    public required string ExternalIdentity { get; set; }

    /// <summary>
    /// The channel scheme for delivering the notification.
    /// </summary>
    /// <seealso cref="NotificationChannelExt"/>
    [Required]
    [JsonPropertyName("channelSchema")]
    [DefaultValue(NotificationChannelExt.Email)]
    public override required NotificationChannelExt ChannelSchema { get; set; } = NotificationChannelExt.Email;
}
