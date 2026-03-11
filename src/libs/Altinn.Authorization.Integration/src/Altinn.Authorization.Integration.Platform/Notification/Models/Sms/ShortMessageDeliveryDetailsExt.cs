using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Notifications.Models.Sms;

/// <summary>
/// Represents delivery details for an SMS including recipient, content, and delivery parameters.
/// </summary>
public record ShortMessageDeliveryDetailsExt
{
    /// <summary>
    /// The recipient's phone number in international format.
    /// </summary>
    [Required]
    [JsonPropertyName("phoneNumber")]
    public required string PhoneNumber { get; init; }

    /// <summary>
    /// The time-to-live duration, specified in seconds.
    /// </summary>
    [Required]
    [JsonPropertyName("timeToLiveInSeconds")]
    public required int TimeToLiveInSeconds { get; init; }

    /// <summary>
    /// The content and sender information.
    /// </summary>
    [Required]
    [JsonPropertyName("smsSettings")]
    public required ShortMessageContentExt ShortMessageContent { get; init; }
}
