using System.Collections.Generic;
using System.Threading.Tasks;
using Bottlecap.Net.Notifications.Data;
using System;
using System.Linq;

namespace Bottlecap.Net.Notifications.EF
{
    public class NotificationRepository : INotificationRepository
    {
        private const int MAXIMUM_NOTIFICATIONS_COUNT = 24;

        private readonly IDataContext _context;
        public NotificationRepository(IDataContext context)
        {
            _context = context;
        }

        public async Task<INotificationData> AddAsync(string notificationType, string transportType, object recipients, object content)
        {
            var data = new NotificationData()
            {
                Content = content,
                CreationTimestamp = DateTime.UtcNow,
                NotificationType = notificationType,
                TransportType = transportType,
                State = NotificationState.Created,
                Recipients = recipients,
                RetryCount = 0
            };

            var savedNotification = await _context.Notifications.AddAsync(data);

            var numberOfChanged = await _context.SaveChangesAsync();
            return numberOfChanged > 0 ? savedNotification.Entity : null;
        }

        public Task<IEnumerable<INotificationData>> GetPendingNotificationsAsync()
        {
            return Task.FromResult<IEnumerable<INotificationData>>(_context.Notifications.Where(x => 
                (
                    (x.State == NotificationState.Created || x.State == NotificationState.WaitingForRetry || x.State == NotificationState.TransporterNotFound) &&
                    (x.NextExecutionTimestamp == null || x.NextExecutionTimestamp <= DateTime.UtcNow)
                )
            )
            .OrderBy(x => x.LastUpdatedTimestamp)
            .Take(MAXIMUM_NOTIFICATIONS_COUNT));
        }

        public async Task UpdateAsync(long id, NotificationState state, int retryCount, string failureDetail, DateTime? nextExecutionTimestamp)
        {
            var data = await _context.Notifications.FindAsync(id);
            if (data != null)
            {
                data.State = state;
                data.RetryCount = retryCount;
                data.FailureDetail = failureDetail;
                data.NextExecutionTimestamp = nextExecutionTimestamp;
                data.LastUpdatedTimestamp = DateTime.UtcNow;

                _context.Notifications.Update(data);

                await _context.SaveChangesAsync();
            }
        }
    }
}
