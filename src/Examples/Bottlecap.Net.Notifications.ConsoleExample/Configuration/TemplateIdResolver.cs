using Bottlecap.Net.Notifications.Transporters.SendGrid;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.ConsoleExample.Configuration
{
    /// <summary>
    /// Template resolver. This is used to determine our SendGrid template id dependent on
    /// our notification type.
    /// 
    /// If you integrate SendGrid transporter, then you'll need either this or IEmailResolver to
    /// determine our email content.
    /// </summary>
    public class TemplateIdResolver : ITemplateIdResolver
    {
        private readonly string _templateId;
        public TemplateIdResolver(string templateId)
        {
            _templateId = templateId;
        }

        public Task<string> GetTemplateIdAsync(string notificationType)
        {
            return Task.FromResult(_templateId);
        }
    }
}
