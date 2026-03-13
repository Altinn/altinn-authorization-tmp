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

public class ResourceRequestAcceptedNotification(IAltinnNotification notification, IEntityService entityService) : IOutboxHandler
{
    public async Task Handle(OutboxMessage message, CancellationToken cancellationToken)
    {
        var (recipient, sender, resources) = await GetContext(message, cancellationToken);

        var response = await notification.Send(
            new()
            {
                IdempotencyId = $"auth_resource_request_accept_{recipient.Id}",
                Recipient = await CreateRecipient(recipient, sender, resources, cancellationToken),
                RequestedSendTime = DateTime.UtcNow,
            },
            cancellationToken);

        if (response.IsProblem)
        {
            throw new InvalidOperationException(response.ProblemDetails.Detail);
        }
    }

    private async Task<(Entity Recipient, Entity Approver, IEnumerable<string> Resources)> GetContext(OutboxMessage message, CancellationToken cancellationToken)
    {
        var content = JsonSerializer.Deserialize<List<ResourceRequestAcceptedNotificationMessage>>(message.Data);
        if (content is null || content.Count == 0)
        {
            throw new InvalidOperationException("Data is empty. Can't send notification without content.");
        }

        var recipients = content.GroupBy(m => m.RecipientId);
        if (recipients.Count() != 1)
        {
            throw new InvalidOperationException("Outbox message contains multiple recipients, should contain only one.");
        }

        var approvers = recipients.Single();
        var approver = content.GroupBy(m => m.AcceptorId);
        if (approver.Count() != 1)
        {
            throw new InvalidOperationException("Outbox message contains multiple senders, should contain only one.");
        }

        var sender = approver.Single();
        var entityRecipient = await entityService.GetEntity(approvers.Key, cancellationToken);
        if (entityRecipient is null)
        {
            throw new InvalidOperationException($"Recipient entity with id '{approvers.Key}' not found.");
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

    private static async Task<NotificationRecipientExt> CreateRecipient(Entity recipient, Entity approver, IEnumerable<string> resources, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipient);

        if (recipient.TypeId == EntityTypeConstants.Person)
        {
            var emailContent = new StringBuilder();
            emailContent.AppendLine($"<p>{approver.Name} har akseptert din forespørsel om følgende fullmakter.</p>");
            emailContent.AppendLine("<ul>");
            foreach (var resource in resources)
            {
                emailContent.AppendLine($"<li>{resource}</li>");
            }

            emailContent.AppendLine("</ul>");
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
                        Subject = "Altinn Godkjent Tilgangsforespørsel",
                        Body = emailContent.ToString()
                    }
                }
            };
        }
        else if (recipient.TypeId == EntityTypeConstants.Organization)
        {
            var emailContent = new StringBuilder();
            emailContent.AppendLine($"<p>{approver.Name} med Org.nr {recipient.OrganizationIdentifier} har akseptert din forespørsel om følgende fullmakter.</p>");
            emailContent.AppendLine("<ul>");
            foreach (var resource in resources)
            {
                emailContent.AppendLine($"<li>{resource}</li>");
            }

            emailContent.AppendLine("</ul>");
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
                        Subject = "Altinn Godkjent Tilgangsforespørsel",
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
public class ResourceRequestAcceptedNotificationMessage
{
    /// <summary>
    /// Entity ID of the acceptor, either person or organization.
    /// </summary>
    public Guid AcceptorId { get; set; }

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
