using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters.SendGrid
{
    /// <summary>
    /// Interface for resolving a notification type into a SendGrid template id.
    /// This is required if you are wanting to generate emails via SendGrid's templating where the notification
    /// content will be sent as dynamic content.
    /// 
    /// If you are wanting to do the latter, you need to implement IEmailResolver.
    /// </summary>
    public interface ITemplateIdResolver
    {
        Task<string> GetTemplateIdAsync(string notificationType);
    }
}
