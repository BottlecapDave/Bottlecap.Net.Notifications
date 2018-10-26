using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters
{
    public interface INotificationTransporter<TRecipient>
    {
        string TransporterType { get; }

        INotificationRecipientResolver<TRecipient> RecipientResolver { get; }

        Task<IEnumerable<string>> SendAsync(string notificationType, object recipients, object content);
    }
}
