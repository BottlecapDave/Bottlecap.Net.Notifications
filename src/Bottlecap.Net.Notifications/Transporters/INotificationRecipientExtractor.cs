using Bottlecap.Net.Notifications.Data;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters
{
    public interface INotificationRecipientExtractor
    {
        Task<object> GetRecipientsAsync(IUser user, string notificationType, string transporterType);
    }
}
