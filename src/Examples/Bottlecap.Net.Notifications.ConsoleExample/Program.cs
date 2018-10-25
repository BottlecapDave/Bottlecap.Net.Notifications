using Bottlecap.Net.Notifications.EF;
using Bottlecap.Net.Notifications.Services;
using Bottlecap.Net.Notifications.Transporters.SendGrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bottlecap.Net.Notifications.ConsoleExample
{
    public class Program
    {
        private class NotificationContent
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        static void Main(string[] args)
        {
            var services = Setup();

            var notificationService = services.GetService<INotificationService>();

            //notificationService.ScheduleAndExecuteAsync("test-notification-console",
            notificationService.ScheduleAndExecuteAsync("test-notification-console",
                new NotificationContent()
            {
                FirstName = "Hello",
                LastName = "World"
            },
            new User()).Wait();
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
            var emailTransporter = new SendGridTransporter(new SendGridOptions()
            {
                ApiKey = sendGridApiToken,
                FromEmailAddress = fromEmailAddress,
                FromEmailName = fromEmailName
            },
            new EmailNotificationRecipientResolver(toEmailAddress),
            templateIdResolver: new TemplateIdResolver(templateId));

            // Setup notifications
            services.AddNotificationServiceWithEF<DatabaseContext>(transporters: emailTransporter);

            return services.BuildServiceProvider();
        }
    }
}
