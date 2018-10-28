using Bottlecap.Net.Notifications;
using Bottlecap.Net.Notifications.Data;
using Bottlecap.Net.Notifications.Services;
using Bottlecap.Net.Notifications.Transporters;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Bottlecap.Net.Notifications
{
    public class NotificationServiceTests
    {
        [Fact]
        public async Task ScheduleAsync_When_ContextIsNull_Then_ArgumentNullExceptionThrown()
        {
            // Arrange
            var mock = new MockNotificationService();
            var recipient = new Recipient();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => mock.Service.ScheduleAsync(null, recipient));
        }

        [Fact]
        public async Task ScheduleAsync_When_RecipientIsNull_Then_ArgumentNullExceptionThrown()
        {
            // Arrange
            var mock = new MockNotificationService();
            var context = new NotificationContext();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => mock.Service.ScheduleAsync(context, null));
        }

        public class Recipient
        {

        }

        public class NotificationContext : INotificationContent
        {
            public string NotificationType { get { return "test-notification"; } }

            public object Content { get; set; }
        }

        private class MockNotificationService
        {
            public Mock<INotificationRepository> MockNotificationRepository { get; private set; }

            public Mock<INotificationTransportManager<Recipient>> MockNotificationTransportManager { get; private set; }

            public NotificationService<Recipient> Service { get; private set; }

            public NotificationServiceOptions Options { get; private set; }

            public MockNotificationService()
            {
                Options = new NotificationServiceOptions();
                MockNotificationRepository = new Mock<INotificationRepository>();
                MockNotificationTransportManager = new Mock<INotificationTransportManager<Recipient>>();

                Service = new NotificationService<Recipient>(MockNotificationRepository.Object,
                                                             MockNotificationTransportManager.Object,
                                                             Options);
            }
        }
    }
}
