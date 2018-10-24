using Bottlecap.Net.Notifications.Services;
using Bottlecap.Net.Notifications.Transporters;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bottlecap.Net.Notifications.EF
{
    public static class ServiceCollectionExtensions
    {
        public static void SetupEFNotificationService<TDataContext>(this IServiceCollection serviceCollection,
                                                                    Action<NotificationServiceOptions> options,
                                                                    params INotificationTransporter[] transporters)
            where TDataContext : class, IDataContext
        {
            serviceCollection.AddScoped<IDataContext>(service => service.GetService<TDataContext>());

            Bottlecap.Net.Notifications.ServiceCollectionExtensions.SetupNotificationService<NotificationRepository>(serviceCollection,
                                                                                                                     options,
                                                                                                                     transporters);
        }

        public static void SetupEFNotificationService<TDataContext>(this IServiceCollection serviceCollection,
                                                                    params INotificationTransporter[] transporters)
            where TDataContext : class, IDataContext
        {
            serviceCollection.SetupEFNotificationService<TDataContext>(null, transporters);
        }
    }
}
