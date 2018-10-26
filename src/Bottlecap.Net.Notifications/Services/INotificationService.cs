using Bottlecap.Net.Notifications.Data;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Services
{
    public interface INotificationService<TRecipient>
    {
        Task<INotificationData> ScheduleAsync(INotificationContext context, TRecipient recipient);
        
        Task<NotifyStatus> ScheduleAndExecuteAsync(INotificationContext context, TRecipient recipient);

        Task<long> ExecuteAsync();
    }
}
