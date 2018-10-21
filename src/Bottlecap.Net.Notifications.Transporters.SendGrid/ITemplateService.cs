using Bottlecap.Net.Notifications.Transporters.Extractors.Emails;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters.SendGrid
{
    public interface ITemplateService
    {
        Task<Email> GenerateEmailAsync(string type, object content);
    }
}
