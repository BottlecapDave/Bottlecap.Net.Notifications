using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Schedulers
{
    public interface IUserSettingsService
    {
        Task<object> GetDestinationAsync(IUser user, string key, string category);
    }
}
