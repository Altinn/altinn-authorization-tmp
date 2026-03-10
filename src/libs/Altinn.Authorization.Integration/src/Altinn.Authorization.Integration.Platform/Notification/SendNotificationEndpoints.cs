using System.Text.Json.Serialization;

namespace Altinn.Authorization.Integration.Platform.Notification;

/// <summary>
/// Client for sending notifications through the Altinn Notifications API.
/// </summary>
public partial class AltinnNotificationClient
{
    /// <inheritdoc/>
    public async Task<PlatformResponse<NotificationResponseModel>> Send(
        NotificationRequestModel model,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Action<HttpRequestMessage>> request = [
            RequestComposer.WithHttpVerb(HttpMethod.Post),
            RequestComposer.WithPayload(model),
            RequestComposer.WithSetUri(NotificationOptions.Value.Endpoint, "/notifications/api/v1/future/orders"),
            RequestComposer.WithPlatformAccessToken(async () => await TokenGenerator.CreatePlatformAccessToken(cancellationToken))
        ];

        var response = await HttpClient.SendAsync(RequestComposer.New([.. request]), cancellationToken);

        return ResponseComposer.Handle<NotificationResponseModel>(
            response,
            ResponseComposer.DeserializeResponseOnSuccess,
            ResponseComposer.DeserializeProblemDetailsOnUnsuccessStatusCode
        );
    }
}

public class NotificationResponseModel
{
    [JsonPropertyName("notificationOrderId")]
    public string NotificationOrderId { get; set; }

    [JsonPropertyName("notification")]
    public NotificationModel Notification { get; set; }

    public class NotificationModel
    {
        [JsonPropertyName("shipmentId")]
        public string ShipmentId { get; set; }

        [JsonPropertyName("sendersReference")]
        public string SendersReference { get; set; }

        [JsonPropertyName("reminders")]
        public List<ReminderModel> Reminders { get; set; }
    }

    public class ReminderModel
    {
        [JsonPropertyName("shipmentId")]
        public string ShipmentId { get; set; }

        [JsonPropertyName("sendersReference")]
        public string SendersReference { get; set; }
    }
}

public class NotificationRequestModel
{
    [JsonPropertyName("idempotencyId")]
    public string IdempotencyId { get; set; }

    [JsonPropertyName("sendersReference")]
    public string SendersReference { get; set; }

    [JsonPropertyName("associationDialogporten")]
    public AssociationDialogportenModel AssociationDialogporten { get; set; }

    [JsonPropertyName("recipient")]
    public RecipientModel Recipient { get; set; }

    [JsonPropertyName("reminders")]
    public List<Reminder> Reminders { get; set; }

    public class Reminder
    {
        [JsonPropertyName("conditionEndpoint")]
        public string ConditionEndpoint { get; set; }

        [JsonPropertyName("sendersReference")]
        public string SendersReference { get; set; }

        [JsonPropertyName("delayDays")]
        public int DelayDays { get; set; }

        [JsonPropertyName("recipient")]
        public RecipientModel Recipient { get; set; }
    }

    public class SmsSettingsModel
    {
        [JsonPropertyName("sendingTimePolicy")]
        public string SendingTimePolicy { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("sender")]
        public string Sender { get; set; }
    }

    public class AssociationDialogportenModel
    {
        [JsonPropertyName("dialogueId")]
        public string DialogueId { get; set; }

        [JsonPropertyName("transmissionId")]
        public string TransmissionId { get; set; }
    }

    public class EmailSettingsModel
    {
        [JsonPropertyName("sendingTimePolicy")]
        public string SendingTimePolicy { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }
    }

    public class RecipientModel
    {
        [JsonPropertyName("recipientPerson")]
        public RecipientPersonModel RecipientPerson { get; set; }
    }

    public class RecipientPersonModel
    {
        [JsonPropertyName("nationalIdentityNumber")]
        public string NationalIdentityNumber { get; set; }

        [JsonPropertyName("resourceId")]
        public string ResourceId { get; set; }

        [JsonPropertyName("ignoreReservation")]
        public bool IgnoreReservation { get; set; }

        [JsonPropertyName("channelSchema")]
        public string ChannelSchema { get; set; }

        [JsonPropertyName("smsSettings")]
        public SmsSettingsModel SmsSettings { get; set; }

        [JsonPropertyName("emailSettings")]
        public EmailSettingsModel EmailSettings { get; set; }
    }
}
