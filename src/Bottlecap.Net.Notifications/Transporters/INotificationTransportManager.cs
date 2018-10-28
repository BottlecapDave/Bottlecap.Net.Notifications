using System.Collections.Generic;

namespace Bottlecap.Net.Notifications.Transporters
{
    /// <summary>
    /// Manages all registered notification transporters.
    /// </summary>
    /// <typeparam name="TRecipient">The type of recipient the notifications are being sent to.</typeparam>
    public interface INotificationTransportManager<TRecipient>
    {
        /// <summary>
        /// Register the provided transporter
        /// </summary>
        /// <param name="transporter">The transporter to be registered.</param>
        void Register(INotificationTransporter<TRecipient> transporter);
        
        /// <summary>
        /// Retrieve the transporter registered against the specified type.
        /// </summary>
        /// <param name="transportType">The type of transporter to retrieve.</param>
        /// <returns>The associated transporter.</returns>
        INotificationTransporter<TRecipient> Get(string transportType);

        /// <summary>
        /// Get all registered transporters.
        /// </summary>
        /// <returns>The collection of registered transporters.</returns>
        IEnumerable<INotificationTransporter<TRecipient>> GetTransporters();
    }
}
