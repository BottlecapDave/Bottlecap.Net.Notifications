using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters.Resolvers.Emails
{
    public interface IEmailNotificationRecipientResolver<TRecipient> : INotificationRecipientResolver<TRecipient>
    {
        new Task<EmailRecipients> ResolveAsync(TRecipient recipient, INotificationContent notificationContent, string transporterType);
    }
}
