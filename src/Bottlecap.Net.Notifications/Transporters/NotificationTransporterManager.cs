using System;
using System.Collections.Generic;
using System.Text;

namespace Bottlecap.Net.Notifications.Transporters
{
    public class NotificationTransporterManager : INotificationTransporterManager
    {
        private readonly Dictionary<string, INotificationTransporter> _registeredTransporters = new Dictionary<string, INotificationTransporter>();

        public NotificationTransporterManager()
        {
        }

        public INotificationTransporter Get(string category)
        {
            INotificationTransporter transporter;
            if (_registeredTransporters.TryGetValue(category, out transporter) == false)
            {
                throw new ArgumentException($"A transporter for category '{transporter.Category}' has not been registered");
            }

            return transporter;
        }

        public IEnumerable<string> GetCategories()
        {
            return _registeredTransporters.Keys;
        }

        public void Register(INotificationTransporter transporter)
        {
            if (_registeredTransporters.ContainsKey(transporter.Category))
            {
                throw new ArgumentException($"A transporter for category '{transporter.Category}' has already been registered");
            }

            _registeredTransporters.Add(transporter.Category, transporter);
        }


    }
}
