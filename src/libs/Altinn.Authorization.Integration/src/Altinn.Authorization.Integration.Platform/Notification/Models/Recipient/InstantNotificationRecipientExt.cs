using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Altinn.Notifications.Models.Sms;

namespace Altinn.Notifications.Models.Recipient;

/// <summary>
/// Represents recipient information for an urgent notification delivery.
/// </summary>
public record InstantNotificationRecipientExt
{
    /// <summary>
    /// The SMS delivery details including recipient, content, and delivery parameters.
    /// </summary>
    [Required]
    [JsonPropertyName("recipientSms")]
    public required ShortMessageDeliveryDetailsExt ShortMessageDeliveryDetails { get; init; }
}
