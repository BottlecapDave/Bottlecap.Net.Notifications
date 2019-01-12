using System.Threading;
using System.Threading.Tasks;
using Bottlecap.Net.Notifications.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Bottlecap.Net.Notifications.Data;
using Xunit;
using System.Linq;

namespace UnitTests.Bottlecap.Net.Notifications.EF
{
    public class NotificationRepositoryTests
    {
        [Fact]
        public async Task AddAsync_When_NotificationsAreAdded_Then_AllDataIsSet()
        {
            // Arrange
            var mock = new MockNotificationRepository();
            var data = new CreatableNotification[]
            {
                new CreatableNotification()
                {
                    Content = new { Hello = "World" },
                    NotificationType = "Email",
                    Recipients = new { To = "test@test.com" },
                    TransportType = "Email"
                }
            };

            // Act
            var result = await mock.Repository.AddAsync(data);

            Assert.NotNull(result);
            Assert.Single(result);

            var actualData = result.First();
            Assert.Equal(data[0].Content, actualData.Content);
            Assert.Equal(data[0].NotificationType, actualData.NotificationType);
            Assert.Equal(data[0].Recipients, actualData.Recipients);
            Assert.Equal(data[0].TransportType, actualData.TransportType);
            Assert.True(actualData.Id > 0, "Id did not seem to be set");
            Assert.Equal(NotificationState.Created, actualData.State);
        }

        private class MockNotificationRepository
        {
            public NotificationRepository Repository { get; private set; }

            public MockNotificationRepository()
            {
                var services = new ServiceCollection();
                services.AddDbContext<TestDataContext>(opt => opt.UseInMemoryDatabase("TestNotifications"));
                var provider = services.BuildServiceProvider();

                Repository = new NotificationRepository(provider.GetService<TestDataContext>());
            }
        }

        private class TestDataContext : DbContext, IDataContext
        {
            public TestDataContext(DbContextOptions options)
                : base(options)
            {

            }

            public DbSet<NotificationData> Notifications { get; set; }
        }
    }
}
