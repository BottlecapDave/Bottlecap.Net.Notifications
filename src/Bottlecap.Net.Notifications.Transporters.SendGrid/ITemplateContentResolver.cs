using Bottlecap.Net.Notifications.Transporters.Resolvers.Emails;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters.SendGrid
{
    public interface ITemplateContentResolver
    {
        Task<Email> GenerateEmailAsync(string notificationType, object content);
    }
}
