using Bottlecap.Net.Notifications.Schedulers;
using System;
using System.Collections.Generic;

namespace Bottlecap.Net.Notifications.Transporters
{
    public class NotificationTransportManager : INotificationTransportManager
    {
        private readonly Dictionary<string, Tuple<INotificationTransporter, IUserSettingsService>> _registeredTransporters = new Dictionary<string, Tuple<INotificationTransporter, IUserSettingsService>>();

        public NotificationTransportManager()
        {
        }

        public INotificationTransporter GetTransporter(string category)
        {
            Tuple<INotificationTransporter, IUserSettingsService> transporter;
            if (_registeredTransporters.TryGetValue(category, out transporter) == false)
            {
                throw new ArgumentException($"A transporter for category '{category}' has not been registered");
            }

            return transporter.Item1;
        }

        public IUserSettingsService GetUserSettingsService(string category)
        {
            Tuple<INotificationTransporter, IUserSettingsService> transporter;
            if (_registeredTransporters.TryGetValue(category, out transporter) == false)
            {
                throw new ArgumentException($"A transporter for category '{category}' has not been registered");
            }

            return transporter.Item2;
        }

        public IEnumerable<string> GetCategories()
        {
            return _registeredTransporters.Keys;
        }

        public void Register(INotificationTransporter transporter, IUserSettingsService settingsService)
        {
            if (_registeredTransporters.ContainsKey(transporter.Category))
            {
                throw new ArgumentException($"A transporter for category '{transporter.Category}' has already been registered");
            }

            _registeredTransporters.Add(transporter.Category, new Tuple<INotificationTransporter, IUserSettingsService>(transporter, settingsService));
        }
    }
}
