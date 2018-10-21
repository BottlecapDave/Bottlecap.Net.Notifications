using Bottlecap.Net.Notifications.Data;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters.Extractors.Emails
{
    public interface IEmailNotificationRecipientExtractor : INotificationRecipientExtractor
    {
        new Task<EmailRecipients> GetRecipientsAsync(IUser user, string notificationType, string transporterType);
    }
}
