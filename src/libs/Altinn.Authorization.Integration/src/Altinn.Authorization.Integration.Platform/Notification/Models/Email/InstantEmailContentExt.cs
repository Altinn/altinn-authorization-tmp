using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Notifications.Models.Email;

/// <summary>
/// Represents the content and sender information for an email.
/// </summary>
public record InstantEmailContentExt
{
    /// <summary>
    /// The subject of the email.
    /// </summary>
    [Required]
    [JsonPropertyName("subject")]
    public required string Subject { get; init; }

    /// <summary>
    /// The body content of the email.
    /// </summary>
    [Required]
    [JsonPropertyName("body")]
    public required string Body { get; init; }

    /// <summary>
    /// The sender's email address.
    /// </summary>
    [JsonPropertyName("senderEmailAddress")]
    public string? SenderEmailAddress { get; init; }

    /// <summary>
    /// The content type of the body (Plain or Html).
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="EmailContentTypeExt.Plain"/>.
    /// Determines how email clients will render the body content.
    /// </remarks>
    [JsonPropertyName("contentType")]
    [DefaultValue(EmailContentTypeExt.Plain)]
    public EmailContentTypeExt ContentType { get; set; } = EmailContentTypeExt.Plain;
}
