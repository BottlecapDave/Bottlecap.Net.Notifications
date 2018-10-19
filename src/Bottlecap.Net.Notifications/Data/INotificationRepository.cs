using Bottlecap.Net.Notifications.Schedulers;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Data
{
    public interface INotificationRepository
    {
        Task AddAsync(string key, IUser user, object content);
    }
}
