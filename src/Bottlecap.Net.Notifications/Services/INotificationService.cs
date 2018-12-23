using Bottlecap.Net.Notifications.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Services
{
    /// <summary>
    /// The service for scheduling and sending notifications. This is the thing you will be interacting with.
    /// </summary>
    /// <typeparam name="TRecipient">The type of recupient the notifications are being sent to.</typeparam>
    public interface INotificationService<TRecipient>
    {
        /// <summary>
        /// Schedule the provided notification for all applicable transporters for the provided recipient. 
        /// 
        /// The purpose of this is that the scheduled notification will be executed at a later date by an
        /// external service.
        /// </summary>
        /// <param name="content">The content of the notification including the notification type and any related data.</param>
        /// <param name="recipient">The recipient the notification is to be sent to</param>
        /// <returns>The collection of notification records that were created.</returns>
        Task<IEnumerable<INotificationData>> ScheduleAsync(INotificationContent content, TRecipient recipient);

        /// <summary>
        /// Schedule the provided notification for all applicable transporters for the provided recipient and attempt to send them straight away.
        /// 
        /// This method should be used if you want the notifications to ideally be sent at the point of calling it.
        /// </summary>
        /// <param name="content">The content of the notification including the notification type and any related data.</param>
        /// <param name="recipient">The recipient the notification is to be sent to</param>
        /// <returns>The status of the creation and sending of the notification.</returns>
        Task<NotifyStatus> ScheduleAndExecuteAsync(INotificationContent content, TRecipient recipient);

        /// <summary>
        /// Attempt to send any scheduled notifications using their specified transporters.
        /// </summary>
        /// <returns>The number of notifications that were successfully sent.</returns>
        Task<long> ExecuteAsync();
    }
}
