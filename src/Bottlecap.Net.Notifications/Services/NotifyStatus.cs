namespace Bottlecap.Net.Notifications.Services
{
    public enum NotifyStatus
    {
        /// <summary>
        /// The notifications were successfully created and sent.
        /// </summary>
        Successful,
        
        /// <summary>
        /// The notifications were successfully scheduled, but one or more were not sent.
        /// </summary>
        Scheduled,

        /// <summary>
        /// The notifications were neither sent nor scheduled successfully.
        /// </summary>
        Failed
    }
}