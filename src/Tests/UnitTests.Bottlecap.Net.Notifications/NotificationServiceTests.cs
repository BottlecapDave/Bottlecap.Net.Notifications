using Bottlecap.Net.Notifications;
using Bottlecap.Net.Notifications.Data;
using Bottlecap.Net.Notifications.Services;
using Bottlecap.Net.Notifications.Transporters;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Bottlecap.Net.Notifications
{
    public class NotificationServiceTests
    {
        #region ScheduleAsync

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
            var content = new NotificationContent();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => mock.Service.ScheduleAsync(content, null));
        }

        [Fact]
        public async Task ScheduleAsync_When_NoTransportersRegistered_Then_EmptyArray()
        {
            // Arrange
            var mock = new MockNotificationService();
            var recipient = new Recipient();
            var content = new NotificationContent();

            // Act
            var result = await mock.Service.ScheduleAsync(content, recipient);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ScheduleAsync_When_TransporterPresent_TranspoterResolverIsNull_Then_InvalidOpertationExceptionThrown()
        {
            // Arrange
            var mock = new MockNotificationService();
            var recipient = new Recipient();
            var content = new NotificationContent();

            mock.SetupTransporters();
            mock.MockNotificationTransporter.Setup(x => x.RecipientResolver).Returns<INotificationRecipientResolver<Recipient>>(null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => mock.Service.ScheduleAsync(content, recipient));
        }

        [Fact]
        public async Task ScheduleAsync_When_TransporterPresent_RecipientResolverReturnsNull_Then_EmptyArray()
        {
            // Arrange
            var mock = new MockNotificationService();
            var recipient = new Recipient();
            var content = new NotificationContent();

            mock.SetupTransporters();
            mock.MockNotificationRecipientResolver.Setup(x => x.ResolveAsync(recipient, content, mock.MockNotificationTransporter.Object.TransporterType))
                                                  .Returns(Task.FromResult<object>(null));

            // Act
            var result = await mock.Service.ScheduleAsync(content, recipient);

            // Assert
            Assert.Empty(result);

            mock.MockNotificationRecipientResolver.Verify(x => x.ResolveAsync(recipient, content, mock.MockNotificationTransporter.Object.TransporterType), Times.Once);
        }

        [Fact]
        public async Task ScheduleAsync_When_TransporterPresent_RecipientResolverReturnsObject_Then_NotificationSaved()
        {
            // Arrange
            var mock = new MockNotificationService();
            var recipient = new Recipient();
            var content = new NotificationContent();
            var resolverResult = new ResolverResult()
            {
                ToAddress = "test@test.com"
            };

            var expectedAddedNotifications = new INotificationData[]
            {
                new NotificationData()
            };

            mock.SetupTransporters();
            mock.MockNotificationRecipientResolver.Setup(x => x.ResolveAsync(recipient, content, mock.MockNotificationTransporter.Object.TransporterType))
                                                  .Returns(Task.FromResult<object>(resolverResult));


            mock.MockNotificationRepository.Setup(x => x.AddAsync(It.Is<IEnumerable<CreatableNotification>>((creatables) => creatables.Count() == 1 &&
                                                                                                                            creatables.FirstOrDefault(item => item.TransportType == mock.MockNotificationTransporter.Object.TransporterType &&
                                                                                                                                                              item.Recipients == resolverResult &&
                                                                                                                                                              item.NotificationType == content.NotificationType &&
                                                                                                                                                              item.Content == content) != null)))
                                           .Returns(Task.FromResult<IEnumerable<INotificationData>>(expectedAddedNotifications));

            // Act
            var result = await mock.Service.ScheduleAsync(content, recipient);

            // Assert
            Assert.Single(result);
            Assert.Equal(expectedAddedNotifications, result);

            mock.MockNotificationRecipientResolver.Verify(x => x.ResolveAsync(recipient, content, mock.MockNotificationTransporter.Object.TransporterType), Times.Once);
            mock.MockNotificationRepository.Verify(x => x.AddAsync(It.IsAny<IEnumerable<CreatableNotification>>()), Times.Once);
        }

        #endregion

        public class Recipient
        {

        }

        public class NotificationContent : INotificationContent
        {
            public string NotificationType { get { return "test-notification"; } }

            public object Content { get; set; }
        }

        public class ResolverResult
        {
            public string ToAddress { get; set; }
        }

        public class NotificationData : INotificationData
        {
            public long Id { get; set; }
            public string NotificationType { get; set; }
            public string TransportType { get; set; }
            public object Recipients { get; set; }
            public object Content { get; set; }
            public NotificationState State { get; set; }
            public string FailureDetail { get; set; }
            public int RetryCount { get; set; }
            public DateTime? NextExecutionTimestamp { get; set; }
            public DateTime CreationTimestamp { get; set; }
            public DateTime? LastUpdatedTimestamp { get; set; }
        }

        private class MockNotificationService
        {
            public Mock<INotificationRepository> MockNotificationRepository { get; private set; }

            public Mock<INotificationTransportManager<Recipient>> MockNotificationTransportManager { get; private set; }

            
            public Mock<INotificationTransporter<Recipient>> MockNotificationTransporter { get; private set; }

            public Mock<INotificationRecipientResolver<Recipient>> MockNotificationRecipientResolver { get; private set; }

            public NotificationService<Recipient> Service { get; private set; }

            public NotificationServiceOptions Options { get; private set; }

            public MockNotificationService()
            {
                Options = new NotificationServiceOptions();
                MockNotificationRepository = new Mock<INotificationRepository>();
                MockNotificationTransportManager = new Mock<INotificationTransportManager<Recipient>>();
                MockNotificationTransporter = new Mock<INotificationTransporter<Recipient>>();
                MockNotificationRecipientResolver = new Mock<INotificationRecipientResolver<Recipient>>();

                MockNotificationTransporter.Setup(x => x.RecipientResolver).Returns(MockNotificationRecipientResolver.Object);
                MockNotificationTransporter.Setup(x => x.TransporterType).Returns("email");

                Service = new NotificationService<Recipient>(MockNotificationRepository.Object,
                                                             MockNotificationTransportManager.Object,
                                                             Options);
            }

            public void SetupTransporters()
            {
                MockNotificationTransportManager.Setup(x => x.GetTransporters()).Returns(new INotificationTransporter<Recipient>[] 
                {
                    MockNotificationTransporter.Object
                });
            }
        }
    }
}
