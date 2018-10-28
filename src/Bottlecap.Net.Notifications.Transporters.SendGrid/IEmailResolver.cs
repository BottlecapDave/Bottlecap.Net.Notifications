using Bottlecap.Net.Notifications.Transporters.Resolvers.Emails;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters.SendGrid
{
    /// <summary>
    /// Interface for resolving a notification into an email that can be sent. 
    /// This is required if you are wanting to generate emails locally for SendGrid instead of using their
    /// templating service. 
    /// 
    /// If you are wanting to do the latter, you need to implement ITemplateIdResolver.
    /// </summary>
    public interface IEmailResolver
    {
        Task<Email> GenerateEmailAsync(string notificationType, object content);
    }
}
