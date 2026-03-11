using System.Text.Json;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Integration.Platform.Notification;

namespace Altinn.AccessMgmt.Core.Outbox;

public class ResourceRequestNotification(IAltinnNotification notification) : IOutboxHandler
{
    public async Task Handle(OutboxMessage message, CancellationToken cancellationToken)
    {
        var content = JsonSerializer.Deserialize<List<ResourceRequestNotificationMessage>>(message.Data);
        var response = await notification.Send(
            new()
            {
                IdempotencyId = $"auth_{content.FirstOrDefault()?.To.ToString()}",
                Recipient = new()
                {
                },
            },
            cancellationToken);

        if (response.IsProblem)
        {
            throw new InvalidOperationException(response.ProblemDetails.Detail);
        }
    }
}

public class ResourceRequestNotificationMessage
{
    public Guid From { get; set; }

    public Guid To { get; set; }

    public string Resource { get; set; }
}
