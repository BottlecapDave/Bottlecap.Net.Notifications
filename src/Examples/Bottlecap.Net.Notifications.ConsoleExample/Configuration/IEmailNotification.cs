namespace Bottlecap.Net.Notifications.ConsoleExample.Configuration
{
    /// <summary>
    /// Interface used to determine if a notification is to be sent by emails. 
    /// 
    /// This is just one way we can have our email resolver determine if the notification is to be
    /// sent via SendGrid.
    /// </summary>
    public interface IEmailNotification
    {
    }
}
