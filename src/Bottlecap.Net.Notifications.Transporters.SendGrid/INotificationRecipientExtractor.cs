using Bottlecap.Net.Notifications.Data;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters.SendGrid
{
    public interface INotificationRecipientExtractor : Bottlecap.Net.Notifications.Transporters.INotificationRecipientExtractor
    {
        new Task<EmailRecipients> GetRecipientsAsync(IUser user, string notificationType, string transporterType);
    }
}
