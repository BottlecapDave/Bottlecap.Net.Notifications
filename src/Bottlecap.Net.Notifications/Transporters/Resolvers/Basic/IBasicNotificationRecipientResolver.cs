using Bottlecap.Net.Notifications.Data;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters.Resolvers.Basic
{
    public interface IBasicNotificationRecipientExtractor<TRecipient> : INotificationRecipientResolver<TRecipient>
    {
        new Task<string> ResolveAsync(TRecipient user, string notificationType, string transporterType);
    }
}
