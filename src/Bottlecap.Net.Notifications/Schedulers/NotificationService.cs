using Bottlecap.Net.Notifications.Data;
using Bottlecap.Net.Notifications.Transporters;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Schedulers
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationTransportManager _manager;

        public NotificationService(INotificationRepository notificationRepository,
                                   INotificationTransportManager manager)
        {
            _notificationRepository = notificationRepository;
            _manager = manager;
        }

        public async Task<bool> ScheduleAsync(string notificationType, object content, IUser user)
        {
            foreach (var transporter in _manager.GetTransporters())
            {
                var destination = await transporter.RecipientExtractor.GetRecipientsAsync(user, notificationType, transporter.TransporterType);
                if (destination != null)
                {
                    await _notificationRepository.AddAsync(notificationType, transporter.TransporterType, destination, content);
                }
            }

            return true;
        }
    }
}
