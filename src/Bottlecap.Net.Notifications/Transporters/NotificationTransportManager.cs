using System;
using System.Collections.Generic;

namespace Bottlecap.Net.Notifications.Transporters
{
    public class NotificationTransportManager : INotificationTransportManager
    {
        private readonly Dictionary<string, INotificationTransporter> _registeredTransporters = new Dictionary<string, INotificationTransporter>();

        public INotificationTransporter Get(string transporterType)
        {
            INotificationTransporter transporter;
            if (_registeredTransporters.TryGetValue(transporterType, out transporter) == false)
            {
                throw new ArgumentException($"A transporter for transport type '{transporterType}' has not been registered");
            }

            return transporter;
        }

        public IEnumerable<INotificationTransporter> GetTransporters()
        {
            return _registeredTransporters.Values;
        }

        public void Register(INotificationTransporter transporter)
        {
            if (_registeredTransporters.ContainsKey(transporter.TransporterType))
            {
                throw new ArgumentException($"A transporter for transport type '{transporter.TransporterType}' has already been registered");
            }

            _registeredTransporters.Add(transporter.TransporterType, transporter);
        }
    }
}
