using System.Collections.Generic;

namespace Bottlecap.Net.Notifications.Transporters
{
    public interface INotificationTransporterManager
    {
        void Register(INotificationTransporter transporter);

        INotificationTransporter Get(string category);

        IEnumerable<string> GetCategories();
    }
}
