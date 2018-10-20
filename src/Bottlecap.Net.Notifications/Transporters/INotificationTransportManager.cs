using System.Collections.Generic;

namespace Bottlecap.Net.Notifications.Transporters
{
    public interface INotificationTransportManager
    {
        void Register(INotificationTransporter transporter);
        
        INotificationTransporter Get(string transportType);

        IEnumerable<INotificationTransporter> GetTransporters();
    }
}
