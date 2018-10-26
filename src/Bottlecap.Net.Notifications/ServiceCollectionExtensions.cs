using Bottlecap.Net.Notifications.Data;
using Bottlecap.Net.Notifications.Services;
using Bottlecap.Net.Notifications.Transporters;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bottlecap.Net.Notifications
{
    public static class ServiceCollectionExtensions
    {
        public static void AddNotificationService<TRecipient, TNotificationRepository>(this IServiceCollection serviceCollection,
                                                                           Action<NotificationServiceOptions> optionsBuilder = null,
                                                                           params INotificationTransporter<TRecipient>[] transporters)
            where TNotificationRepository : class, INotificationRepository
        {
            serviceCollection.AddScoped<INotificationRepository, TNotificationRepository>();
            serviceCollection.AddScoped<INotificationService<TRecipient>>(factory =>
            {
                var serviceOptions = new NotificationServiceOptions();
                if (optionsBuilder != null)
                {
                    optionsBuilder(serviceOptions);
                }

                var manager = new NotificationService<TRecipient>(factory.GetService<INotificationRepository>(),
                                                      factory.GetService<INotificationTransportManager<TRecipient>>(),
                                                      serviceOptions);

                return manager;
            });
            serviceCollection.AddScoped<INotificationTransportManager<TRecipient>>(factory =>
            {
                var manager = new NotificationTransportManager<TRecipient>();
                foreach (var transporter in transporters)
                {
                    manager.Register(transporter);
                }

                return manager;
            });
        }
    }
}
