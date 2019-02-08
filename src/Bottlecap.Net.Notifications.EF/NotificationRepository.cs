using System.Collections.Generic;
using System.Threading.Tasks;
using Bottlecap.Net.Notifications.Data;
using System;
using System.Linq;

namespace Bottlecap.Net.Notifications.EF
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDataContext _context;
        public NotificationRepository(IDataContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<INotificationData>> AddAsync(IEnumerable<CreatableNotification> notifications)
        {
            var data = notifications.Select(notification =>
            {
                return new NotificationData()
                {
                    Content = notification.Content,
                    CreationTimestamp = DateTime.UtcNow,
                    NotificationType = notification.NotificationType,
                    TransportType = notification.TransportType,
                    State = NotificationState.Created,
                    Recipients = notification.Recipients,
                    RetryCount = 0
                };
            })
            .ToArray(); // If we don't to array the collection, then our ids won't auto populate

            await _context.Notifications.AddRangeAsync(data);

            var numberOfChanged = await _context.SaveChangesAsync();
            return numberOfChanged > 0 ? data : null;
        }

        public Task<IEnumerable<INotificationData>> GetPendingNotificationsAsync(DateTime latestCreationTimestamp, int? numberOfItemsExecute = null)
        {
            IQueryable<INotificationData> items = _context.Notifications.Where(x =>
                (
                    (x.State == NotificationState.Created || x.State == NotificationState.WaitingForRetry || x.State == NotificationState.TransporterNotFound) &&
                    x.CreationTimestamp <= latestCreationTimestamp &&
                    (x.NextExecutionTimestamp == null || x.NextExecutionTimestamp <= DateTime.UtcNow)
                )
            )
            .OrderBy(x => x.LastUpdatedTimestamp);

            if (numberOfItemsExecute.HasValue)
            {
                items = items.Take(numberOfItemsExecute.Value);
            }

            return Task.FromResult<IEnumerable<INotificationData>>(items.ToArray());
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
