using Bottlecap.Net.Notifications.Data;
using Bottlecap.Net.Notifications.Transporters;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Schedulers
{
    public class NotificationScheduler : INotificationScheduler
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationTransportManager _manager;

        public NotificationScheduler(INotificationRepository notificationRepository,
                                     INotificationTransportManager manager)
        {
            _notificationRepository = notificationRepository;
            _manager = manager;
        }

        public async Task<bool> ScheduleAsync(string key, object content, IUser user)
        {
            foreach (var category in _manager.GetCategories())
            {
                var settings = _manager.GetUserSettingsService(category);
                var destination = await settings.GetDestinationAsync(user, key, category);
                if (destination != null)
                {
                    await _notificationRepository.AddAsync(key, user, content);
                }
            }

            return true;
        }
    }
}
