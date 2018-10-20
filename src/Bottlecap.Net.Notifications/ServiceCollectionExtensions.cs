using Bottlecap.Net.Notifications.Data;
using Bottlecap.Net.Notifications.Services;
using Bottlecap.Net.Notifications.Transporters;
using Microsoft.Extensions.DependencyInjection;

namespace Bottlecap.Net.Notifications
{
    public static class ServiceCollectionExtensions
    {
        public static void SetupNotificationService<TNotificationRepository>(this IServiceCollection serviceCollection, 
                                                                             params INotificationTransporter[] transporters)
            where TNotificationRepository : class, INotificationRepository
        {
            serviceCollection.AddScoped<INotificationRepository, TNotificationRepository>();
            serviceCollection.AddScoped<INotificationService, NotificationService>();
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
