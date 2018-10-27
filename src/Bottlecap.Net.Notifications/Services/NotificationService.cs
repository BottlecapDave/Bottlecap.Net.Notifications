using Bottlecap.Net.Notifications.Data;
using Bottlecap.Net.Notifications.Transporters;
using System.Threading.Tasks;
using System;
using System.Linq;

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

        public async Task<NotifyStatus> ScheduleAndExecuteAsync(INotificationContext context, TRecipient recipient)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            else if (recipient == null)
            {
                throw new ArgumentNullException(nameof(recipient));
            }

            var result = await ScheduleAsync(context, recipient);
            if (result != null)
            {
                return await NotifyAsync(result);
            }

            return NotifyStatus.Failed;
        }

        public async Task<INotificationData> ScheduleAsync(INotificationContext context, TRecipient recipient)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            else if (recipient == null)
            {
                throw new ArgumentNullException(nameof(recipient));
            }

            foreach (var transporter in _manager.GetTransporters())
            {
                if (transporter.RecipientResolver != null)        
                { 
                    var destination = await transporter.RecipientResolver.ResolveAsync(recipient, context.NotificationType, transporter.TransporterType);
                    if (destination != null)
                    {
                        return await _repository.AddAsync(context.NotificationType, transporter.TransporterType, destination, context.Content);
                    }
                }
            }

            return null;
        }

        private async Task<NotifyStatus> NotifyAsync(INotificationData data)
        {
            bool wasSuccessful = false;
            var failureDetail = String.Empty;
            try
            {
                var transporter = _manager.Get(data.TransportType);
                if (transporter == null)
                {
                    throw new TransporterNotFoundException(data.TransportType);
                }

                var errors = await transporter.SendAsync(data.NotificationType, data.Recipients, data.Content);
                wasSuccessful = errors.Any() == false;
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
