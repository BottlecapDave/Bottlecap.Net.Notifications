using Bottlecap.Net.Notifications.Data;
using Bottlecap.Net.Notifications.Services;
using Bottlecap.Net.Notifications.Transporters;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bottlecap.Net.Notifications
{
    public static class ServiceCollectionExtensions
    {
        public static void AddNotificationService<TNotificationRepository>(this IServiceCollection serviceCollection,
                                                                           Action<NotificationServiceOptions> optionsBuilder = null,
                                                                           params INotificationTransporter[] transporters)
            where TNotificationRepository : class, INotificationRepository
        {
            serviceCollection.AddScoped<INotificationRepository, TNotificationRepository>();
            serviceCollection.AddScoped<INotificationService>(factory =>
            {
                var serviceOptions = new NotificationServiceOptions();
                if (optionsBuilder != null)
                {
                    optionsBuilder(serviceOptions);
                }

                var manager = new NotificationService(factory.GetService<INotificationRepository>(),
                                                      factory.GetService<INotificationTransportManager>(),
                                                      serviceOptions);

                return manager;
            });
            serviceCollection.AddScoped<INotificationTransportManager>(factory =>
            {
                var manager = new NotificationTransportManager();
                foreach (var transporter in transporters)
                {
                    manager.Register(transporter);
                }

                return manager;
            });
        }
    }
}
