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

            await _context.Notifications.AddAsync(data);

            var result = await _context.SaveChangesAsync();
            return result > 0 ? data : null;
        }

        public Task<IEnumerable<INotificationData>> GetPendingNotificationsAsync()
        {
            return Task.FromResult<IEnumerable<INotificationData>>(_context.Notifications.Where(x => 
                (
                    (x.State == NotificationState.Created || x.State == NotificationState.WaitingForRetry) &&
                    (x.NextExecutionTimestamp == null || x.NextExecutionTimestamp <= DateTime.UtcNow)
                )
            )
            .OrderBy(x => x.LastUpdatedTimestamp)
            .Take(MAXIMUM_NOTIFICATIONS_COUNT));
        }

        public Task UpdateAsync(long id, NotificationState state, int retryCount, DateTime? nextExecutionTimestamp)
        {
            var data = _context.Notifications.FirstOrDefault(x => x.Id == id);
            if (data != null)
            {
                data.State = state;
                data.RetryCount = retryCount;
                data.NextExecutionTimestamp = nextExecutionTimestamp;
                data.LastUpdatedTimestamp = DateTime.UtcNow;

                _context.Notifications.Update(data);
            }

            return Task.FromResult(true);
        }
    }
}
