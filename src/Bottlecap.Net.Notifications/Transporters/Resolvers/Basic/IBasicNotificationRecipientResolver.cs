using Bottlecap.Net.Notifications.Data;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters.Resolvers.Basic
{
    public interface IBasicNotificationRecipientExtractor : INotificationRecipientResolver
    {
        new Task<string> ResolveAsync(IUser user, string notificationType, string transporterType);
    }
}
