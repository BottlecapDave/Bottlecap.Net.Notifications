using Bottlecap.Net.Notifications.Services;
using Bottlecap.Net.Notifications.Transporters;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bottlecap.Net.Notifications.EF
{
    public static class ServiceCollectionExtensions
    {
        public static void AddNotificationServiceWithEF<TRecipient, TDataContext>(this IServiceCollection serviceCollection,
                                                                      Action<NotificationServiceOptions> options = null,
                                                                      params INotificationTransporter<TRecipient>[] transporters)
            where TDataContext : class, IDataContext
        {
            serviceCollection.AddScoped<IDataContext>(service => service.GetService<TDataContext>());

            Bottlecap.Net.Notifications.ServiceCollectionExtensions.AddNotificationService<TRecipient, NotificationRepository>(serviceCollection,
                                                                                                                   options,
                                                                                                                   transporters);
        }
    }
}
