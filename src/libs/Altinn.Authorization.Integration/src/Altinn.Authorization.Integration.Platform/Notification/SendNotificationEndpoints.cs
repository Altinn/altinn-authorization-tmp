using System.Text.Json.Serialization;

namespace Altinn.Authorization.Integration.Platform.Notification;

/// <summary>
/// Client for sending notifications through the Altinn Notifications API.
/// </summary>
public partial class AltinnNotificationClient
{
    /// <inheritdoc/>
    public async Task<PlatformResponse<SMSNotificationResponseModel>> SendSms(
        SMSNotificationRequestModel model,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Action<HttpRequestMessage>> request = [
            RequestComposer.WithHttpVerb(HttpMethod.Post),
            RequestComposer.WithPayload(model),
            RequestComposer.WithSetUri(NotificationOptions.Value.Endpoint, "/notifications/api/v1/future/orders/instant/sms"),
            RequestComposer.WithPlatformAccessToken(async () => await TokenGenerator.CreatePlatformAccessToken(cancellationToken))
        ];

        var response = await HttpClient.SendAsync(RequestComposer.New([.. request]), cancellationToken);

        return ResponseComposer.Handle<SMSNotificationResponseModel>(
            response,
            ResponseComposer.DeserializeResponseOnSuccess,
            ResponseComposer.DeserializeProblemDetailsOnUnsuccessStatusCode
        );
    }

    /// <inheritdoc/>
    public async Task<PlatformResponse<EmailNotificationResponseModel>> SendEmail(
        EmailNotificationRequestModel model,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Action<HttpRequestMessage>> request = [
            RequestComposer.WithHttpVerb(HttpMethod.Post),
            RequestComposer.WithPayload(model),
            RequestComposer.WithSetUri(NotificationOptions.Value.Endpoint, "/notifications/api/v1/future/orders/instant/email"),
            RequestComposer.WithPlatformAccessToken(async () => await TokenGenerator.CreatePlatformAccessToken(cancellationToken))
        ];

        var response = await HttpClient.SendAsync(RequestComposer.New([.. request]), cancellationToken);

        return ResponseComposer.Handle<EmailNotificationResponseModel>(
            response,
            ResponseComposer.DeserializeResponseOnSuccess,
            ResponseComposer.DeserializeProblemDetailsOnUnsuccessStatusCode
        );
    }
}

/// <summary>
/// Represents the response returned when an SMS notification order is created.
/// </summary>
public class SMSNotificationResponseModel
{
    /// <summary>
    /// Gets or sets the identifier of the notification order.
    /// </summary>
    [JsonPropertyName("notificationOrderId")]
    public string NotificationOrderId { get; set; }

    /// <summary>
    /// Gets or sets the created SMS notification.
    /// </summary>
    [JsonPropertyName("notification")]
    public NotificationModel Notification { get; set; }

    /// <summary>
    /// Represents details about the created SMS notification.
    /// </summary>
    public class NotificationModel
    {
        /// <summary>
        /// Gets or sets the shipment identifier for the notification.
        /// </summary>
        [JsonPropertyName("shipmentId")]
        public string ShipmentId { get; set; }

        /// <summary>
        /// Gets or sets the sender's reference associated with the notification.
        /// </summary>
        [JsonPropertyName("sendersReference")]
        public string SendersReference { get; set; }
    }
}

/// <summary>
/// Represents a request to send an SMS notification.
/// </summary>
public class SMSNotificationRequestModel
{
    /// <summary>
    /// Gets or sets the idempotency identifier used to prevent duplicate requests.
    /// </summary>
    [JsonPropertyName("idempotencyId")]
    public string IdempotencyId { get; set; }

    /// <summary>
    /// Gets or sets the sender's reference for the notification request.
    /// </summary>
    [JsonPropertyName("sendersReference")]
    public string SendersReference { get; set; }

    /// <summary>
    /// Gets or sets the SMS recipient and message settings.
    /// </summary>
    [JsonPropertyName("recipientSms")]
    public RecipientSmsModel RecipientSms { get; set; }

    /// <summary>
    /// Represents the SMS recipient and associated message settings.
    /// </summary>
    public class RecipientSmsModel
    {
        /// <summary>
        /// Gets or sets the recipient phone number.
        /// </summary>
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the time-to-live for the SMS notification, in seconds.
        /// </summary>
        [JsonPropertyName("timeToLiveInSeconds")]
        public int TimeToLiveInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the SMS-specific settings.
        /// </summary>
        [JsonPropertyName("smsSettings")]
        public SmsSettingsModel SmsSettings { get; set; }

        /// <summary>
        /// Represents the SMS message content and sender.
        /// </summary>
        public class SmsSettingsModel
        {
            /// <summary>
            /// Gets or sets the SMS sender name.
            /// </summary>
            [JsonPropertyName("sender")]
            public string Sender { get; set; }

            /// <summary>
            /// Gets or sets the SMS message body.
            /// </summary>
            [JsonPropertyName("body")]
            public string Body { get; set; }
        }
    }
}

/// <summary>
/// Represents a request to send an email notification.
/// </summary>
public class EmailNotificationRequestModel
{
    /// <summary>
    /// Gets or sets the idempotency identifier used to prevent duplicate requests.
    /// </summary>
    [JsonPropertyName("idempotencyId")]
    public string IdempotencyId { get; set; }

    /// <summary>
    /// Gets or sets the sender's reference for the notification request.
    /// </summary>
    [JsonPropertyName("sendersReference")]
    public string SendersReference { get; set; }

    /// <summary>
    /// Gets or sets the email recipient and message settings.
    /// </summary>
    [JsonPropertyName("recipientEmail")]
    public RecipientEmailModel RecipientEmail { get; set; }

    /// <summary>
    /// Represents the email recipient and associated message settings.
    /// </summary>
    public class RecipientEmailModel
    {
        /// <summary>
        /// Gets or sets the recipient email address.
        /// </summary>
        [JsonPropertyName("emailAddress")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the email-specific settings.
        /// </summary>
        [JsonPropertyName("emailSettings")]
        public EmailSettingsModel EmailSettings { get; set; }

        /// <summary>
        /// Represents the email message content and sender settings.
        /// </summary>
        public class EmailSettingsModel
        {
            /// <summary>
            /// Gets or sets the email subject.
            /// </summary>
            [JsonPropertyName("subject")]
            public string Subject { get; set; }

            /// <summary>
            /// Gets or sets the email body.
            /// </summary>
            [JsonPropertyName("body")]
            public string Body { get; set; }

            /// <summary>
            /// Gets or sets the sender email address.
            /// </summary>
            [JsonPropertyName("senderEmailAddress")]
            public string SenderEmailAddress { get; set; }

            /// <summary>
            /// Gets or sets the content type of the email body.
            /// </summary>
            [JsonPropertyName("contentType")]
            public string ContentType { get; set; }
        }
    }
}

/// <summary>
/// Represents the response returned when an email notification order is created.
/// </summary>
public class EmailNotificationResponseModel
{
    /// <summary>
    /// Gets or sets the identifier of the notification order.
    /// </summary>
    [JsonPropertyName("notificationOrderId")]
    public string NotificationOrderId { get; set; }

    /// <summary>
    /// Gets or sets the created email notification.
    /// </summary>
    [JsonPropertyName("notification")]
    public NotificationRequestModel Notification { get; set; }

    /// <summary>
    /// Represents details about the created email notification.
    /// </summary>
    public class NotificationRequestModel
    {
        /// <summary>
        /// Gets or sets the shipment identifier for the notification.
        /// </summary>
        [JsonPropertyName("shipmentId")]
        public string ShipmentId { get; set; }

        /// <summary>
        /// Gets or sets the sender's reference associated with the notification.
        /// </summary>
        [JsonPropertyName("sendersReference")]
        public string SendersReference { get; set; }
    }
}