using Bottlecap.Net.Notifications.Schedulers;
using System.Collections.Generic;

namespace Bottlecap.Net.Notifications.Transporters
{
    public interface INotificationTransportManager
    {
        void Register(INotificationTransporter transporter, IUserSettingsService userSettingService);

        INotificationTransporter GetTransporter(string category);

        IUserSettingsService GetUserSettingsService(string category);

        IEnumerable<string> GetCategories();
    }
}
