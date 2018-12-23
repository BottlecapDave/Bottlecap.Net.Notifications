using Bottlecap.Net.Notifications.ConsoleExample.Configuration;
using Bottlecap.Net.Notifications.EF;
using Bottlecap.Net.Notifications.Services;
using Bottlecap.Net.Notifications.Transporters.SendGrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bottlecap.Net.Notifications.ConsoleExample
{
    public class Program
    {
        static void Main(string[] args)
        {
            var services = Setup();

            // turn off tracking so we can really see our recipients/content being recorded to the DB and extracted back out again.
            var databaseContext = services.GetService<DatabaseContext>();
            databaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var notificationService = services.GetService<INotificationService<User>>();

            // ScheduleAndExecuteAsync This should be called if you want to schedule the notification and attempt
            // to send it straight away.
            // notificationService.ScheduleAndExecuteAsync();

            notificationService.ScheduleAsync(new TestNotification()
            {
                FirstName = "Hello",
                LastName = "World"
            },
            new User()
            {
                EmailAddress = "" // ADD EMAIL ADDRESS WE'RE WANTING TO SEND OUR NOTIFICATION TO
            }).Wait();

            // This should be called by a notification "service" whose purpose is to
            // send notifications for failed notifications or notifications that have just been scheduled
            var numberOfNotificationsExecuted = notificationService.ExecuteAsync().Result;
            if (numberOfNotificationsExecuted < 1)
            {
                throw new System.InvalidOperationException("Failed to execute notifications");
            }
        }

        private static ServiceProvider Setup()
        {
            var sendGridApiToken = ""; // ADD API TOKEN HERE
            var templateId = ""; // ADD SENDGRID TEMPLATE ID HERE
            
            var fromEmailAddress = ""; // ADD EMAIL ADDRESS HERE
            var fromEmailName = ""; // Add FROM NAME HERE

            var services = new ServiceCollection();

            // Setup in memory Entity Framework
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName: "memorydatabase");
            });

            // Setup notifications
            services.AddNotificationServiceWithEF<User, DatabaseContext>(options =>
            {
                // Setup options for NotificationService
                options.MaximumRetryCount = 10;
                options.PendingNotificationOffsetInSeconds = 0;
            },
            factory =>
            {
                // Setup SendGrid transporter
                return new SendGridTransporter<User>(new SendGridOptions()
                {
                    ApiKey = sendGridApiToken,
                    FromEmailAddress = fromEmailAddress,
                    FromEmailName = fromEmailName
                },
                new EmailNotificationRecipientResolver(),
                templateIdResolver: new TemplateIdResolver(templateId));
            });

            return services.BuildServiceProvider();
        }
    }
}
