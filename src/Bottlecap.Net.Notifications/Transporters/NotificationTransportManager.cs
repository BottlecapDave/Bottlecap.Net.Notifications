using System;
using System.Collections.Generic;

namespace Bottlecap.Net.Notifications.Transporters
{
    public class NotificationTransportManager<TRecipient> : INotificationTransportManager<TRecipient>
    {
        private readonly Dictionary<string, INotificationTransporter<TRecipient>> _registeredTransporters = new Dictionary<string, INotificationTransporter<TRecipient>>();

        public INotificationTransporter<TRecipient> Get(string transporterType)
        {
            INotificationTransporter<TRecipient> transporter;
            if (_registeredTransporters.TryGetValue(transporterType, out transporter) == false)
            {
                throw new ArgumentException($"A transporter for transport type '{transporterType}' has not been registered");
            }

            return transporter;
        }

        public IEnumerable<INotificationTransporter<TRecipient>> GetTransporters()
        {
            return _registeredTransporters.Values;
        }

        public void Register(INotificationTransporter<TRecipient> transporter)
        {
            if (_registeredTransporters.ContainsKey(transporter.TransporterType))
            {
                throw new ArgumentException($"A transporter for transport type '{transporter.TransporterType}' has already been registered");
            }

            _registeredTransporters.Add(transporter.TransporterType, transporter);
        }
    }
}
