using System;

namespace Bottlecap.Net.Notifications.Data
{
    public interface INotificationData
    {
        long Id { get; set; }

        string NotificationType { get; set; }

        string TransportType { get; set; }

        object Recipients { get; set; }

        object Content { get; set; }

        NotificationState State { get; set; }

        int RetryCount { get; set; }

        DateTime? NextExecutionTimestamp { get; set; }

        DateTime CreationTimestamp { get; set; }

        DateTime? LastUpdatedTimestamp { get; set; }
    }
}