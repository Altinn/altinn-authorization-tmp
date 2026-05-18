using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Altinn.AccessMgmt.Core.Extensions;
using Altinn.AccessMgmt.Core.Notifications;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Base;
using Altinn.Authorization.Integration.Platform.Notification;
using Altinn.Authorization.Integration.Platform.Notification.Models;
using Altinn.Authorization.Integration.Platform.Notification.Models.Email;
using Altinn.Authorization.Integration.Platform.Notification.Models.Recipient;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace Altinn.AccessMgmt.Core.Outbox;

public class InstanceAddedNotificationHandler(
    AppDbContext db,
    IAltinnNotification notification,
    IFeatureManager featureManager,
    IEntityService entityService) : IOutboxHandler
{
    public async Task<OutboxStatus> Handle(OutboxMessage message, CancellationToken cancellationToken)
    {
        if (await featureManager.IsDisabledAsync(AccessMgmtFeatureFlags.OutboxInstanceAddedNotify, cancellationToken))
        {
            db.OutboxMessageLogs.Add(message, $"Feature flag '{AccessMgmtFeatureFlags.OutboxInstanceAddedNotify}' is disabled.");
            await db.SaveChangesAsync(cancellationToken);
            return OutboxStatus.Completed;
        }

        var (recipient, requester, instances, idempotencyId) = await UnwrapMessage(message, cancellationToken);

        if (!instances.Any())
        {
            db.OutboxMessageLogs.Add(message, $"Lists of instances are empty. Access was most likely added and immediately removed.");
            await db.SaveChangesAsync(cancellationToken);
            return OutboxStatus.Completed;
        }

        var content = new NotificationOrderChainRequestExt()
        {
            IdempotencyId = idempotencyId,
            SendersReference = idempotencyId,
            Recipient = CreateRecipient(recipient, requester, instances),
        };

        var response = await notification.Send(content, cancellationToken);

        if (response.IsProblem)
        {
            // Contact information for organization / person is missing
            if (response.ProblemDetails?.ErrorCode.ToString() == "NOT-00001")
            {
                db.OutboxMessageLogs.Add(
                    message,
                    response.ProblemDetails?.Title ?? "Missing contact information for recipient(s)"
                );

                await db.SaveChangesAsync(cancellationToken);
                return OutboxStatus.Completed;
            }

            var errorMessage = $@"Failed to send notification.
                Payload: {JsonSerializer.Serialize(content)}
                CorrelationId: {Activity.Current?.TraceId}
                Status Code: {response.StatusCode}
                Problem Title: {response.ProblemDetails?.Title}
                Problem Details: {response.ProblemDetails?.Detail}
                Problem Instance: {response.ProblemDetails?.Instance}
                Problem Type: {response.ProblemDetails?.Type}
                Problem Error Code: {response.ProblemDetails?.ErrorCode}
                Problem Extensions: {JsonSerializer.Serialize(response.ProblemDetails?.Extensions ?? new Dictionary<string, object>())}";

            db.OutboxMessageLogs.Add(
                message,
                errorMessage
            );

            await db.SaveChangesAsync(cancellationToken);

            return OutboxStatus.Failed;
        }

        return OutboxStatus.Completed;
    }

    private async Task<(Entity To, Entity From, IEnumerable<string> Instances, string IdempotencyId)> UnwrapMessage(OutboxMessage message, CancellationToken cancellationToken)
    {
        var content = JsonSerializer.Deserialize<InstanceAddedNotificationMessage>(message.Data);
        if (content is null)
        {
            throw new InvalidOperationException("Data is empty. Can't send notification without content.");
        }

        var entityFrom = await entityService.GetEntity(content.FromId, cancellationToken);
        if (entityFrom is null)
        {
            throw new InvalidOperationException($"From entity with id '{content.FromId}' not found.");
        }

        var entityTo = await entityService.GetEntity(content.ToId, cancellationToken);
        if (entityTo is null)
        {
            throw new InvalidOperationException($"To entity with id '{content.ToId}' not found.");
        }

        return (
            entityFrom,
            entityTo,
            content.Instances,
            $"auth_{AccessAddedNotification.Handler}_{entityFrom.Id}_{entityTo.Id}_{message.CreatedAt.Ticks}"
        );
    }

    private static NotificationRecipientExt CreateRecipient(Entity from, Entity to, IEnumerable<string> instances)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        var pronoun = to.TypeId == EntityTypeConstants.Person ? "Du" : "Dere";
        var subject = $"{pronoun} har fått fullmakt i Altinn";

        return new NotificationRecipientExt
        {
            RecipientPerson = new RecipientPersonExt
            {
                NationalIdentityNumber = to.PersonIdentifier,
                ChannelSchema = NotificationChannelExt.Email,
                EmailSettings = new EmailSendingOptionsExt
                {
                    Subject = subject,
                    Body = MailContent(from, to, instances),
                    ContentType = EmailContentTypeExt.Html,
                    SendingTimePolicy = SendingTimePolicyExt.Anytime,
                }
            }
        };

        throw new InvalidOperationException("to entity type must be of type <Person | Organization>");
    }

    private static string MailContent(Entity from, Entity to, IEnumerable<string> instances)
    {
        var access = new StringBuilder();
        if (instances is { } && instances.Count() > 0)
        {
            access.Append(@"
                <p>
                    <strong>Delt fra innboks:</strong>
                </p>
            ");
            access.Append("<ul>");
            foreach (var instance in instances)
            {
                access.Append($"<li>{instance}</li>");
            }

            access.Append("</ul>");
        }

        var ingress = Ingress(from, to);

        return @$"
            <p>
                Hei,
            </p>
            {ingress}
            {access}
            <p>
                Logg inn på Altinn, gå til innboks for å finne meldingen.
            </p>
            <p>
                Med vennlig hilsen,<br>
                Altinn
            </p>
            <em>
                Denne meldingen er automatisk generert. Svar til denne adressen vil ikke bli behandlet.
            </em>";

        static string Ingress(Entity from, Entity to)
        {
            if (from.TypeId == EntityTypeConstants.Person && to.TypeId == EntityTypeConstants.Person)
            {
                return /*HTML*/ $@"
                <p>
                    Du har fått følgende fullmakter i Altinn på vegne av {from.Name}, fødselsdato {from.DateOfBirth}
                </p>";
            }

            if (from.TypeId == EntityTypeConstants.Organization && to.TypeId == EntityTypeConstants.Person)
            {
                return /*HTML*/ $@"
                <p>
                    Du har fått følgende fullmakter i Altinn på vegne av {from.Name}, organisasjonsnummer {from.OrganizationIdentifier}
                </p>
            ";
            }

            if (from.TypeId == EntityTypeConstants.Person && to.TypeId == EntityTypeConstants.Organization)
            {
                return /*HTML*/ $@"
                <p>
                    {to.Name} har fått følgende fullmakter i Altinn på vegne av {from.Name}, fødselsdato {from.DateOfBirth}
                </p>
            ";
            }

            if (from.TypeId == EntityTypeConstants.Organization && to.TypeId == EntityTypeConstants.Organization)
            {
                return /*HTML*/ $@"
                <p>
                    {to.Name} har fått følgende fullmakter i Altinn på vegne av {from.Name}, organisasjonsnummer {from.OrganizationIdentifier}
                </p>
            ";
            }

            throw new InvalidOperationException("from and to entity type must be of type <Person | Organization>");
        }
    }
}

public class InstanceAddedNotificationMessage
{
    public Guid FromId { get; set; }

    public Guid ToId { get; set; }

    public List<Instance> Instances { get; set; } = [];

    public int Updated { get; set; }

    public class Instance
    {
        public List<string> InstanceIds { get; set; } = [];

        public Guid ResourceId { get; set; }
    }
}
