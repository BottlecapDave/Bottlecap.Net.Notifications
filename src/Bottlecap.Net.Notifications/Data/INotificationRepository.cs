using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Data
{
    public interface INotificationRepository
    {
        Task<IEnumerable<INotificationData>> AddAsync(IEnumerable<CreatableNotification> notifications);

        Task<IEnumerable<INotificationData>> GetPendingNotificationsAsync(DateTime latestCreationTimestamp, int? numberOfItemsExecute = null);

        Task UpdateAsync(long id, NotificationState state, int retryCount, string failureDetail, DateTime? nextExecutionTimestamp);
    }
}
