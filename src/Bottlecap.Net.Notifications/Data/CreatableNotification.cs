namespace Bottlecap.Net.Notifications.Data
{
    public class CreatableNotification
    {
        public string NotificationType { get; set; }
        public string TransportType { get; set; }
        public object Recipients { get; set; }
        public object Content { get; set; }
    }
}
