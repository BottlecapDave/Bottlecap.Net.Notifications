namespace Bottlecap.Net.Notifications.Data
{
    public enum NotificationState
    {
        Created,
        Processing,
        Successful,
        WaitingForRetry,
        Failed,
        TransporterNotFound
    }
}
