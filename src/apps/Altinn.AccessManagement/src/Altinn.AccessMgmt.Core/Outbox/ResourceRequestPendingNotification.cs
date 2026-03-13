using System.Text;
using System.Text.Json;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Integration.Platform.Notification;
using Altinn.Authorization.Integration.Platform.Notification.Models;
using Altinn.Authorization.Integration.Platform.Notification.Models.Email;
using Altinn.Authorization.Integration.Platform.Notification.Models.Recipient;

namespace Altinn.AccessMgmt.Core.Outbox;

public class ResourceRequestPendingNotification(IAltinnNotification notification, IEntityService entityService) : IOutboxHandler
{
    public async Task Handle(OutboxMessage message, CancellationToken cancellationToken)
    {
        var (recipient, sender, resources) = await GetContext(message, cancellationToken);

        var response = await notification.Send(
            new()
            {
                IdempotencyId = $"auth_resource_request_accept_{recipient.Id}",
                Recipient = CreateRecipient(recipient, sender, resources),
                RequestedSendTime = DateTime.UtcNow,
            },
            cancellationToken);

        if (response.IsProblem)
        {
            throw new InvalidOperationException(response.ProblemDetails.Detail);
        }
    }

    private async Task<(Entity Recipient, Entity Sender, IEnumerable<string> Resources)> GetContext(OutboxMessage message, CancellationToken cancellationToken)
    {
        var content = JsonSerializer.Deserialize<List<ResourceRequestPendingNotificationMessage>>(message.Data);
        if (content is null || content.Count == 0)
        {
            throw new InvalidOperationException("Data is empty. Can't send notification without content.");
        }

        var recipients = content.GroupBy(m => m.RecipientId);
        if (recipients.Count() != 1)
        {
            throw new InvalidOperationException("Outbox message contains multiple recipients, should contain only one.");
        }

        var recipient = recipients.Single();
        var senders = content.GroupBy(m => m.RequesterId);
        if (senders.Count() != 1)
        {
            throw new InvalidOperationException("Outbox message contains multiple senders, should contain only one.");
        }

        var sender = senders.Single();
        var entityRecipient = await entityService.GetEntity(recipient.Key, cancellationToken);
        if (entityRecipient is null)
        {
            throw new InvalidOperationException($"Recipient entity with id '{recipient.Key}' not found.");
        }

        var entitySender = await entityService.GetEntity(sender.Key, cancellationToken);
        if (sender is null)
        {
            throw new InvalidOperationException($"Sender entity with id '{sender.Key}' not found.");
        }

        return (
            entityRecipient,
            entitySender,
            content.Select(m => m.Resource).Distinct()
        );
    }

    private static NotificationRecipientExt CreateRecipient(Entity recipient, Entity sender, IEnumerable<string> resources)
    {
        ArgumentNullException.ThrowIfNull(recipient);

        if (recipient.TypeId == EntityTypeConstants.Person)
        {
            var emailContent = new StringBuilder();
            emailContent.AppendLine($"<p>{sender.Name} har bedt om følgende fullmakter fra deg.</p>");
            emailContent.AppendLine("<ul>");
            foreach (var resource in resources)
            {
                emailContent.AppendLine($"<li>{resource}</li>");
            }

            emailContent.AppendLine("</ul>");
            emailContent.AppendLine("<p>Logg inn i Altinn, gå til tilgangsstyring og forespørsler for å behandle forespørselen.</p>");
            emailContent.AppendLine($"<p>Med vennnlig hilsen<b>Altinn</b></p>");

            return new NotificationRecipientExt
            {
                RecipientPerson = new RecipientPersonExt
                {
                    NationalIdentityNumber = recipient.PersonIdentifier,
                    ChannelSchema = NotificationChannelExt.EmailAndSms,
                    ResourceId = "altinn_access_management_hovedadmin",
                    EmailSettings = new EmailSendingOptionsExt
                    {
                        Subject = "Altinn Tilgangsforespørsel",
                        Body = emailContent.ToString()
                    }
                }
            };
        }
        else if (recipient.TypeId == EntityTypeConstants.Organization)
        {
            var emailContent = new StringBuilder();
            emailContent.AppendLine($"<p>{sender.Name} har bedt om følgende fullmakter fra {recipient.Name} med Org.nr {recipient.OrganizationIdentifier}.</p>");
            emailContent.AppendLine("<ul>");
            foreach (var resource in resources)
            {
                emailContent.AppendLine($"<li>{resource}</li>");
            }

            emailContent.AppendLine("</ul>");
            emailContent.AppendLine($"<p>Du mottar denne forespørselen fordi du har tilgangspakken hovedaministrator for {recipient.Name} i Altinn. Logg inn i Altinn velg riktig aktør og gå til tilgangsstyring og forespørsler for å behandle forespørselen.</p>");
            emailContent.AppendLine($"<p>Med vennnlig hilsen<b>Altinn</b></p>");

            return new NotificationRecipientExt
            {
                RecipientOrganization = new RecipientOrganizationExt
                {
                    OrgNumber = recipient.OrganizationIdentifier,
                    ChannelSchema = NotificationChannelExt.EmailPreferred,
                    ResourceId = "altinn_access_management_hovedadmin",
                    EmailSettings = new()
                    {
                        Subject = "Altinn Tilgangsforespørsel",
                        Body = emailContent.ToString()
                    }
                }
            };
        }

        throw new InvalidOperationException("Unsupported party type. not person or organization");
    }
}

/// <summary>
/// Model used for deserializing content of outbox message for resource request notification.
/// </summary>
public class ResourceRequestPendingNotificationMessage
{
    /// <summary>
    /// Entity ID of the requester, either person or organization.
    /// </summary>
    public Guid RequesterId { get; set; }

    /// <summary>
    /// Entity ID of the recipient, either person or organization. 
    /// </summary>
    public Guid RecipientId { get; set; }

    /// <summary>
    /// Name of resource.
    /// </summary>
    public string Resource { get; set; }

    /// <summary>
    /// Used for creating a unique idempotency key and external ref id
    /// </summary>
    public DateTime RefId { get; set; }
}
