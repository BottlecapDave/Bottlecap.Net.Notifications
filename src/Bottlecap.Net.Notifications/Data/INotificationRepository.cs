using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Data
{
    public interface INotificationRepository
    {
        Task<bool> AddAsync(string notificationType, string transportType, object destination, object content);
    }
}
