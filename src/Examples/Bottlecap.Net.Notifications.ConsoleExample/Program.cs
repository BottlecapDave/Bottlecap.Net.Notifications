using Bottlecap.Net.Notifications.EF;
using Bottlecap.Net.Notifications.Services;
using Bottlecap.Net.Notifications.Transporters.SendGrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bottlecap.Net.Notifications.ConsoleExample
{
    public class Program
    {
        private class NotificationContext : INotificationContext
        {
            public string NotificationType { get { return "test-notification-console"; } }
            public NotificationContent Content { get; set; }

            object INotificationContext.Content { get { return Content; } }
        }

        private class NotificationContent
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        static void Main(string[] args)
        {
            var services = Setup();

            var notificationService = services.GetService<INotificationService<User>>();

            // ScheduleAndExecuteAsync This should be called if you want to schedule the notification and attempt
            // to send it straight away.
            // notificationService.ScheduleAndExecuteAsync();

            notificationService.ScheduleAsync(new NotificationContext()
            {
                Content = new NotificationContent()
                {
                    FirstName = "Hello",
                    LastName = "World"
                }
            },
            new User()).Wait();

            // This should be called by a notification "service" whose purpose is to
            // send notifications for failed notifications or notifications that have just been scheduled
            notificationService.ExecuteAsync().Wait();
        }

        private static ServiceProvider Setup()
        {
            var sendGridApiToken = ""; // ADD API TOKEN HERE
            var templateId = ""; // ADD SENDGRID TEMPLATE ID HERE

            var toEmailAddress = ""; // ADD EMAIL ADDRESS HERE
            var fromEmailAddress = ""; // ADD EMAIL ADDRESS HERE
            var fromEmailName = ""; // Add FROM NAME HERE

            var services = new ServiceCollection();

            // Setup in memory Entity Framework
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName: "memorydatabase");
            });

            // Setup transporter
            var emailTransporter = new SendGridTransporter<User>(new SendGridOptions()
            {
                ApiKey = sendGridApiToken,
                FromEmailAddress = fromEmailAddress,
                FromEmailName = fromEmailName
            },
            new EmailNotificationRecipientResolver<User>(toEmailAddress),
            templateIdResolver: new TemplateIdResolver(templateId));

            // Setup notifications
            services.AddNotificationServiceWithEF<User, DatabaseContext>(transporters: emailTransporter);

            return services.BuildServiceProvider();
        }
    }
}
