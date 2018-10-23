using Bottlecap.Net.Notifications.Transporters;
using Microsoft.Extensions.DependencyInjection;

namespace Bottlecap.Net.Notifications.EF
{
    public static class ServiceCollectionExtensions
    {
        public static void SetupNotificationService<TDataContext>(this IServiceCollection serviceCollection, 
                                                                             params INotificationTransporter[] transporters)
            where TDataContext : class, IDataContext
        {
            serviceCollection.AddScoped<IDataContext>(service => service.GetService<TDataContext>());

            Bottlecap.Net.Notifications.ServiceCollectionExtensions.SetupNotificationService<NotificationRepository>(serviceCollection,
                                                                                                                     transporters);
        }
    }
}
