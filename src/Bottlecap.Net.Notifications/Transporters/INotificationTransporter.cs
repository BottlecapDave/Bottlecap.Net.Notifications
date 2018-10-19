using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters
{
    public interface INotificationTransporter
    {
        string Category { get; }

        Task<bool> SendAsync();
    }
}
