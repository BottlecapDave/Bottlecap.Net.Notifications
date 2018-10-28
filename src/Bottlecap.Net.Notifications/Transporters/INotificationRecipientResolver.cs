using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters
{
    /// <summary>
    /// Used to resolve the provided recipient into a recipient object the associated transporter can understand.
    /// 
    /// For example, an email based transporter will use this to resolve the recipient into a collection of email addresses.
    /// </summary>
    /// <typeparam name="TRecipient">The recipient to resolve.</typeparam>
    public interface INotificationRecipientResolver<TRecipient>
    {
        /// <summary>
        /// Resolve the provided recipient into an object the associated transporter can understand, relative to the provided
        /// notification and for the specified transporter.
        /// </summary>
        /// <param name="recipient">The recipient to resolve</param>
        /// <param name="notificationContent">The content of the notification being sent, include the notification type.</param>
        /// <param name="transporterType">The type of transporter the notification is for.</param>
        /// <returns>The recipient object for the specified transporter.</returns>
        Task<object> ResolveAsync(TRecipient recipient, INotificationContent notificationContent, string transporterType);
    }
}
