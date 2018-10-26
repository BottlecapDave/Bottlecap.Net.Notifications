namespace Bottlecap.Net.Notifications
{
    public interface INotificationContext
    {
        string NotificationType { get; }

        object Content { get; }
    }
}
