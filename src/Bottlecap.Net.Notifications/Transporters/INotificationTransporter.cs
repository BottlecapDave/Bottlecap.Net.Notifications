﻿using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.Transporters
{
    public interface INotificationTransporter
    {
        string TransporterType { get; }

        INotificationRecipientExtractor RecipientExtractor { get; }

        Task<bool> SendAsync(string notificationType, object recipients, object content);
    }
}
