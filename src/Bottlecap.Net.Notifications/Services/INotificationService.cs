using Bottlecap.Net.Notifications.Data;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Services
{
    public interface INotificationService
    {
        Task<INotificationData> ScheduleAsync(string notificationType, object content, IUser user);
        
        Task<NotifyStatus> ScheduleAndExecuteAsync(string notificationType, object content, IUser user);

        Task<NotifyStatus> ExecuteAsync();
    }
}
