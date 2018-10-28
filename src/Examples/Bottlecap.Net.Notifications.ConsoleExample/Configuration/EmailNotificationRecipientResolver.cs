using Bottlecap.Net.Notifications.Transporters.Resolvers.Emails;
using Bottlecap.Net.Notifications.Transporters;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.ConsoleExample.Configuration
{
    /// <summary>
    /// Our recipient resolver for our transporters (i.e SendGrid). 
    /// 
    /// This is used to house the logic around extracting email addresses from our recipient, in our case our user instance.
    /// In a real system, we'd also use this resolver to determine if our user wants to receive email notifications or a notification
    /// of this type.
    /// </summary>
    /// <typeparam name="TRecipient"></typeparam>
    public class EmailNotificationRecipientResolver : IEmailNotificationRecipientResolver<User>
    {
        public EmailNotificationRecipientResolver()
        {
        }

        public Task<EmailRecipients> ResolveAsync(User user, INotificationContent notificationContext, string transporterType)
        {
            // We're using IEmailNotification interface to determine if our notification has been marked to be
            // sent via email. If it doesn't implement it, then we don't provide our email.
            if (notificationContext is IEmailNotification)
            {
                return Task.FromResult(new EmailRecipients()
                {
                    To = new string[1] { user.EmailAddress }
                });
            }

            // If our context doesn't implement 
            return Task.FromResult<EmailRecipients>(null);
        }

        async Task<object> INotificationRecipientResolver<User>.ResolveAsync(User user, INotificationContent notificationContext, string transporterType)
        {
            return await ResolveAsync(user, notificationContext, transporterType);
        }
    }
}
