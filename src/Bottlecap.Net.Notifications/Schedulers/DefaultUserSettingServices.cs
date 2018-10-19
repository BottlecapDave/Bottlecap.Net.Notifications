using Bottlecap.Net.Notifications.Transporters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Schedulers
{
    /// <summary>
    /// Default user settings service, which states the user supports all registered transporters.
    /// </summary>
    public class DefaultUserSettingServices : IUserSettingsService
    {
        private readonly INotificationTransportManager _manager;
        public DefaultUserSettingServices(INotificationTransportManager manager)
        {
            _manager = manager;
        }

        public Task<IEnumerable<string>> GetSupportedTransporterCategories(IUser user, string key)
        {
            return Task.FromResult(true);
        }
    }
}
