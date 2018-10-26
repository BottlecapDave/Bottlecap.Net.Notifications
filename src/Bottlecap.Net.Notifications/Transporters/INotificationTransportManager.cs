using System.Collections.Generic;

namespace Bottlecap.Net.Notifications.Transporters
{
    public interface INotificationTransportManager<TRecipient>
    {
        void Register(INotificationTransporter<TRecipient> transporter);
        
        INotificationTransporter<TRecipient> Get(string transportType);

        IEnumerable<INotificationTransporter<TRecipient>> GetTransporters();
    }
}
