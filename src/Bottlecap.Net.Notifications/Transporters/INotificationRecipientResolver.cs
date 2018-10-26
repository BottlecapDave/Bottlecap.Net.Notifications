using Bottlecap.Net.Notifications.Data;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters
{
    public interface INotificationRecipientResolver<TRecipient>
    {
        Task<object> ResolveAsync(TRecipient user, string notificationType, string transporterType);
    }
}
