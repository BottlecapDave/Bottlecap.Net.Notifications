using Bottlecap.Net.Notifications.Data;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters.Extractors.Basic
{
    public interface IBasicNotificationRecipientExtractor : INotificationRecipientExtractor
    {
        new Task<string> GetRecipientsAsync(IUser user, string notificationType, string transporterType);
    }
}
