using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Data
{
    public interface INotificationRepository
    {
        Task<INotificationData> AddAsync(string notificationType, string transportType, object destination, object content);

        Task<IEnumerable<INotificationData>> GetPendingNotificationsAsync();

        Task UpdateAsync(long id, NotificationState state, int retryCount);
    }
}
