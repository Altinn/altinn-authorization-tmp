using System.Text.Json;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Integration.Platform.Notification;

namespace Altinn.AccessMgmt.Core.Outbox;

public class ResourceRequestNotification(IAltinnNotification notification) : IOutboxHandler
{
    public Task Handle(OutboxMessage message, CancellationToken cancellationToken)
    {
        var content = JsonSerializer.Deserialize<List<ResourceRequestNotificationMessage>>(message.Data);
        notification.Send(new()
        {
            IdempotencyId = "",
            SendersReference = "", 
            AssociationDialogporten = new()
            {

            },
            Recipient = new()
            {
                RecipientPerson = new()
                {
                    
                },
            },
            Reminders = [],
        });

        return Task.CompletedTask;
    }
}

public class ResourceRequestNotificationMessage
{
    public Guid From { get; set; }

    public Guid To { get; set; }

    public string Resource { get; set; }
}
