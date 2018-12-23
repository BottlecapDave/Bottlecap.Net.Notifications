using Bottlecap.Net.Notifications.Data;
using Bottlecap.Net.Notifications.Transporters;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Bottlecap.Net.Notifications.Services
{
    public class NotificationService<TRecipient> : INotificationService<TRecipient>
    {
        private readonly INotificationRepository _repository;
        private readonly INotificationTransportManager<TRecipient> _manager;
        private readonly NotificationServiceOptions _options;

        public NotificationService(INotificationRepository repository,
                                   INotificationTransportManager<TRecipient> manager,
                                   NotificationServiceOptions options)
        {
            _repository = repository;
            _manager = manager;
            _options = options;
        }

        public async Task<long> ExecuteAsync()
        {
            var totalSent = 0;
            var notifications = await _repository.GetPendingNotificationsAsync();
            if (notifications != null)
            {
                foreach (var item in notifications)
                {
                    if (await NotifyAsync(item) == NotifyStatus.Successful)
                    {
                        totalSent++;
                    }
                }
            }

            return totalSent;
        }

        public async Task<NotifyStatus> ScheduleAndExecuteAsync(INotificationContent context, TRecipient recipient)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            else if (recipient == null)
            {
                throw new ArgumentNullException(nameof(recipient));
            }

            var notifications = await ScheduleAsync(context, recipient);
            if (notifications != null)
            {
                var result = NotifyStatus.Successful;
                foreach (var notification in notifications)
                {
                    // If we've failed to notify our notification, then we'll tell the caller that all notifications are scheduled.
                    if (await NotifyAsync(notification) != NotifyStatus.Successful)
                    {
                        result = NotifyStatus.Scheduled;
                    }
                }

                return result;
            }

            return NotifyStatus.Failed;
        }

        public async Task<IEnumerable<INotificationData>> ScheduleAsync(INotificationContent content, TRecipient recipient)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            else if (recipient == null)
            {
                throw new ArgumentNullException(nameof(recipient));
            }

            var notificationsToAdd = new List<CreatableNotification>();
            foreach (var transporter in _manager.GetTransporters())
            {
                if (transporter.RecipientResolver == null)        
                {
                    throw new InvalidOperationException($"'{transporter.GetType().Name}' does not have a valid resolver");
                }

                var destination = await transporter.RecipientResolver.ResolveAsync(recipient, content, transporter.TransporterType);
                if (destination != null)
                {
                    notificationsToAdd.Add(new CreatableNotification()
                    {
                        NotificationType = content.NotificationType,
                        TransportType = transporter.TransporterType,
                        Recipients = destination,
                        Content = content
                    });
                }
            }

            return notificationsToAdd.Count > 0 ? await _repository.AddAsync(notificationsToAdd) : new INotificationData[0];
        }

        private async Task<NotifyStatus> NotifyAsync(INotificationData data)
        {
            bool wasSuccessful = false;
            var failureDetail = String.Empty;
            try
            {
                await _repository.UpdateAsync(data.Id, NotificationState.Processing, data.RetryCount, data.FailureDetail, data.NextExecutionTimestamp);

                var transporter = _manager.Get(data.TransportType);
                if (transporter == null)
                {
                    throw new TransporterNotFoundException(data.TransportType);
                }

                var errors = await transporter.SendAsync(data.NotificationType, data.Recipients, data.Content);
                wasSuccessful = errors.Any(x => String.IsNullOrEmpty(x) == false) == false;
                failureDetail = errors?.Aggregate((current, next) => $"{current}. {next}");
            }
            catch (System.Exception ex)
            {
                failureDetail = ex.Message;
            }
            finally
            {
                data.State = wasSuccessful ? NotificationState.Successful : NotificationState.Failed;
                DateTime? nextExecutionTimestamp = null;

                if (wasSuccessful == false &&
                    (_options.MaximumRetryCount == null || data.RetryCount < _options.MaximumRetryCount))
                {
                    data.State = NotificationState.WaitingForRetry;
                    data.RetryCount++;

                    nextExecutionTimestamp = DateTime.UtcNow.AddSeconds(_options.RetryCoolDownInSeconds * data.RetryCount * _options.RetryCoolDownMagnitude);
                }
                
                await _repository.UpdateAsync(data.Id, data.State, data.RetryCount, failureDetail, nextExecutionTimestamp);
            }

            switch (data.State)
            {
                case NotificationState.Successful:
                    return NotifyStatus.Successful;
                case NotificationState.WaitingForRetry:
                    return NotifyStatus.Scheduled;
                default:
                    return NotifyStatus.Failed;
            }
        }
    }
}
