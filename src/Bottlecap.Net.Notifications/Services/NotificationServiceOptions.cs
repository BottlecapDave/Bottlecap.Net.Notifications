namespace Bottlecap.Net.Notifications.Services
{
    public class NotificationServiceOptions
    {
        public int? MaximumRetryCount { get; set; }

        public int RetryCoolDownInSeconds { get; set; }

        public int RetryCoolDownMagnitude { get; set; }

        public int PendingNotificationOffsetInSeconds { get; set; }

        public NotificationServiceOptions()
        {
            RetryCoolDownInSeconds = 60;
            RetryCoolDownMagnitude = 1;
            PendingNotificationOffsetInSeconds = 60;
        }
    }
}