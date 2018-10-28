namespace Bottlecap.Net.Notifications.ConsoleExample.Configuration
{
    /// <summary>
    /// We have one of these for each notification we're wanting to send. 
    /// 
    /// It contains at least our notification type. The other properties are the context
    /// for the notification, which will be provided to all applicable transporters. In our example,
    /// this is SendGrid, where our properties will be provided as dynamic content.
    /// </summary>
    public class TestNotification : INotificationContent, IEmailNotification
    {
        public string NotificationType { get { return "test-notification-console"; } }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
