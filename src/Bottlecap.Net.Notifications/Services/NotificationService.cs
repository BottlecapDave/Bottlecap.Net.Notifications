﻿using Bottlecap.Net.Notifications.Data;
using Bottlecap.Net.Notifications.Transporters;
using System.Threading.Tasks;
using System;

namespace Bottlecap.Net.Notifications.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly INotificationTransportManager _manager;
        private readonly NotificationServiceOptions _options;

        public NotificationService(INotificationRepository repository,
                                   INotificationTransportManager manager,
                                   NotificationServiceOptions options)
        {
            _repository = repository;
            _manager = manager;
            _options = options;
        }

        public async Task<NotifyStatus> NotifyAsync()
        {
            var notifications = await _repository.GetPendingNotificationsAsync();
            if (notifications != null)
            {
                foreach (var item in notifications)
                {
                    await NotifyAsync(item);
                }
            }

            return NotifyStatus.Successful;
        }

        public async Task<NotifyStatus> NotifyAsync(string notificationType, object content, IUser user)
        {
            var result = await ScheduleAsync(notificationType, content, user);
            if (result != null)
            {
                return await NotifyAsync(result);
            }

            return NotifyStatus.Failed;
        }

        public async Task<INotificationData> ScheduleAsync(string notificationType, object content, IUser user)
        {
            foreach (var transporter in _manager.GetTransporters())
            {
                var destination = await transporter.RecipientExtractor.GetRecipientsAsync(user, notificationType, transporter.TransporterType);
                if (destination != null)
                {
                    return await _repository.AddAsync(notificationType, transporter.TransporterType, destination, content);
                }
            }

            return null;
        }

        private async Task<NotifyStatus> NotifyAsync(INotificationData data)
        {
            bool wasSuccessful = false;
            try
            {
                var transporter = _manager.Get(data.TransportType);
                if (transporter == null)
                {
                    throw new TransporterNotFoundException(data.TransportType);
                }

                wasSuccessful = await transporter.SendAsync(data.NotificationType, data.Recipients, data.Content);
            }
            finally
            {
                data.State = wasSuccessful ? NotificationState.Successful : NotificationState.Failed;
                
                if (wasSuccessful == false &&
                    (_options.MaximumRetryCount == null || data.RetryCount < _options.MaximumRetryCount))
                {
                    data.State = NotificationState.WaitingForRetry;
                    data.RetryCount++;
                }
                
                await _repository.UpdateAsync(data.Id, data.State, data.RetryCount);
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
