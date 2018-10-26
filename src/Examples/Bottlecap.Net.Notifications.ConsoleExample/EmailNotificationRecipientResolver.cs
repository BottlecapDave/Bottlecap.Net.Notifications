using Bottlecap.Net.Notifications.Transporters.Resolvers.Emails;
using Bottlecap.Net.Notifications.Data;
using Bottlecap.Net.Notifications.Transporters;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.ConsoleExample
{
    public class EmailNotificationRecipientResolver<TRecipient> : IEmailNotificationRecipientResolver<TRecipient>
    {
        private string _emailAddress;

        public EmailNotificationRecipientResolver(string emailAddress)
        {
            _emailAddress = emailAddress;
        }

        public Task<EmailRecipients> ResolveAsync(TRecipient user, string notificationType, string transporterType)
        {
            return Task.FromResult(new EmailRecipients()
            {
                To = new string[1] { _emailAddress }
            });
        }

        async Task<object> INotificationRecipientResolver<TRecipient>.ResolveAsync(TRecipient user, string notificationType, string transporterType)
        {
            return await ResolveAsync(user, notificationType, transporterType);
        }
    }
}
