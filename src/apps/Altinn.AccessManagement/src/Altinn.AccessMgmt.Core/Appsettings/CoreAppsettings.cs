using Altinn.AccessMgmt.Core.Notifications;
using Altinn.AccessMgmt.Core.Outbox;

namespace Altinn.AccessMgmt.Core.Appsettings;

public class CoreAppsettings
{
    public CoreAppsettings()
    {
    }

    public CoreAppsettings(Action<CoreAppsettings> configureAppsettings)
    {
        if (configureAppsettings is { })
        {
            configureAppsettings(this);
        }
    }

    public RequestOptions Request { get; set; } = new();

    public NotificationsOptions Notifications { get; set; } = new();

    public class NotificationsOptions
    {
        public int RequestReviewedNotifyInSeconds { get; set; } = RequestReviewedNotification.DefaultNotifyInSeconds;

        public int RequestPendingNotifyInSeconds { get; set; } = RequestPendingNotification.DefaultNotifyInSeconds;

        public int RightholderAddedNotifyInSeconds { get; set; } = RightholderAddedNotification.DefaultNotifyInSeconds;

        public int RightholderRemovedNotifyInSeconds { get; set; } = RightholderRemovedNotification.DefaultNotifyInSeconds;

        public int AccessAddedNotifyInSeconds { get; set; } = AccessAddedNotification.DefaultNotifyInSeconds;

        public int AccessRemovedNotifyInSeconds { get; set; } = AccessRemovedNotification.DefaultNotifyInSeconds;

        public int AgentRemovedNotifyInSeconds { get; set; } = AgentRemovedNotification.DefaultNotifyInSeconds;

        public int AgentAddedNotifyInSeconds { get; set; } = AgentAddedNotification.DefaultNotifyInSeconds;

        public int ClientAddedNotifyInSeconds { get; set; } = ClientAddedNotification.DefaultNotifyInSeconds;

        public int ClientRemovedNotifyInSeconds { get; set; } = ClientRemovedNotification.DefaultNotifyInSeconds;
    }

    [Obsolete]
    public class RequestOptions
    {
        public int NotifyRequestApprovedInSeconds { get; set; } = 60 * 15;

        public int NotifyRequestPendingInSeconds { get; set; } = 60 * 15;
    }
}
