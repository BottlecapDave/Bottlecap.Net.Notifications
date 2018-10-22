using Bottlecap.Net.Notifications.Data;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters
{
    public interface INotificationRecipientResolver
    {
        Task<object> ResolveAsync(IUser user, string notificationType, string transporterType);
    }
}
