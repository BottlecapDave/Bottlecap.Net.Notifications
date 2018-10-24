using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters.SendGrid
{
    public interface ITemplateIdResolver
    {
        Task<string> GetTemplateIdAsync(string notificationType);
    }
}
