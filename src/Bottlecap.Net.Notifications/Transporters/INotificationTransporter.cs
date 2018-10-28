using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters
{
    /// <summary>
    /// A transportation mechanism for sending notifications.
    /// </summary>
    /// <typeparam name="TRecipient">The type of recipient having notifications sent to.</typeparam>
    public interface INotificationTransporter<TRecipient>
    {
        /// <summary>
        /// The unique identifier for the transporter.
        /// </summary>
        string TransporterType { get; }

        /// <summary>
        /// The resolver which is used to convert the recipient into something the transporter can understand.
        /// </summary>
        INotificationRecipientResolver<TRecipient> RecipientResolver { get; }

        /// <summary>
        /// Send the notification
        /// </summary>
        /// <param name="notificationType">The type of notification being sent.</param>
        /// <param name="recipients">The recipients having the notification sent to.</param>
        /// <param name="content">The raw contents of the notification.</param>
        /// <returns>Collection of errors that occurred during the sending.</returns>
        Task<IEnumerable<string>> SendAsync(string notificationType, object recipients, object content);
    }
}
