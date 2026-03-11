using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Notifications.Models.Email;

/// <summary>
/// Represents email details for an instant email including recipient and content.
/// </summary>
public record InstantEmailDetailsExt
{
    /// <summary>
    /// The recipient's email address.
    /// </summary>
    [Required]
    [JsonPropertyName("emailAddress")]
    public required string EmailAddress { get; init; }

    /// <summary>
    /// The email content settings.
    /// </summary>
    [Required]
    [JsonPropertyName("emailSettings")]
    public required InstantEmailContentExt EmailSettings { get; init; }
}
