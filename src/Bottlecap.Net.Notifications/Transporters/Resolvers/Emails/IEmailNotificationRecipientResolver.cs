using Bottlecap.Net.Notifications.Data;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters.Resolvers.Emails
{
    public interface IEmailNotificationRecipientResolver : INotificationRecipientResolver
    {
        new Task<EmailRecipients> ResolveAsync(IUser user, string notificationType, string transporterType);
    }
}
