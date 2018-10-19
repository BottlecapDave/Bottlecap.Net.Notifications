using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Schedulers
{
    public interface INotificationScheduler
    {
        Task<bool> ScheduleAsync(string key, object content, IUser user);
    }
}
