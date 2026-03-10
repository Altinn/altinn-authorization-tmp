using Altinn.Register.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.Integration.Platform.Notification;

/// <summary>
/// Client for interacting with the Altinn Register service.
/// </summary>
/// <param name="HttpClientFactory">Factory for creating HTTP clients.</param>
/// <param name="NotificationOptions">Options for configuring the Altinn Register services.</param>
/// <param name="PlatformOptions">Options for configuring platform integration services.</param>
/// <param name="TokenGenerator">Service for generating authentication tokens.</param>
public partial class AltinnNotificationClient(
    IHttpClientFactory HttpClientFactory,
    IOptions<AltinnNotificationOptions> NotificationOptions,
    IOptions<AltinnIntegrationOptions> PlatformOptions,
    ITokenGenerator TokenGenerator
) : IAltinnNotification
{
    private HttpClient HttpClient => HttpClientFactory.CreateClient(PlatformOptions.Value.HttpClientName);
}

/// <summary>
/// Defines operations for sending notifications through the Altinn Notifications service.
/// </summary>
public interface IAltinnNotification
{
    /// <summary>
    /// Sends an SMS notification through the Altinn Notifications service.
    /// </summary>
    /// <param name="model">
    /// The SMS notification request, including idempotency information, sender reference,
    /// recipient phone number, time-to-live, and SMS content.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the platform response
    /// with details about the created SMS notification order.
    /// </returns>
    Task<PlatformResponse<SMSNotificationResponseModel>> SendSms(SMSNotificationRequestModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email notification through the Altinn Notifications service.
    /// </summary>
    /// <param name="model">
    /// The email notification request, including idempotency information, sender reference,
    /// recipient email address, subject, body, sender email address, and content type.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the platform response
    /// with details about the created email notification order.
    /// </returns>
    Task<PlatformResponse<EmailNotificationResponseModel>> SendEmail(EmailNotificationRequestModel model, CancellationToken cancellationToken = default);
}
