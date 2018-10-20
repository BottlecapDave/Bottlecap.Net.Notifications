using Bottlecap.Net.Notifications.Data;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Schedulers
{
    public interface INotificationService
    {
        Task<bool> ScheduleAsync(string notificationType, object content, IUser user);
    }
}
