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
                new MockNotificationData()
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

        #region ExecuteAsync

        [Fact]
        public async Task ExecuteAsync_When_NoNotificationsPresent_Then_ZeroReturned()
        {
            // Arrange
            var mock = new MockNotificationService();
            
            mock.MockNotificationRepository.Setup(x => x.GetPendingNotificationsAsync(It.Is<DateTime>(mock.ExpectedPendingNotificationsTimestampCheck), It.IsAny<int?>())).Returns(Task.FromResult<IEnumerable<INotificationData>>(null));

            // Act
            var result = await mock.Service.ExecuteAsync();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task ExecuteAsync_When_TransporterNotFound_Then_TransporterNotFoundExceptionThrown()
        {
            // Arrange
            var mock = new MockNotificationService();
            var pendingNotifications = new INotificationData[]
            {
                new MockNotificationData()
            };

            var expectedNextSchedule = DateTime.UtcNow.AddSeconds(60);

            mock.MockNotificationRepository.Setup(x => x.GetPendingNotificationsAsync(It.Is<DateTime>(mock.ExpectedPendingNotificationsTimestampCheck), It.IsAny<int?>())).Returns(Task.FromResult<IEnumerable<INotificationData>>(pendingNotifications));

            // Act & Assert
            await Assert.ThrowsAsync<TransporterNotFoundException>(() => mock.Service.ExecuteAsync());

            // Assert
            mock.MockNotificationRepository.Verify(x => x.GetPendingNotificationsAsync(It.Is<DateTime>(mock.ExpectedPendingNotificationsTimestampCheck), It.IsAny<int?>()), Times.Once);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(pendingNotifications[0].Id, 
                                                                      NotificationState.Processing,
                                                                      0,
                                                                      null,
                                                                      null), 
                                                    Times.Once);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(pendingNotifications[0].Id,
                                                                      NotificationState.WaitingForRetry,
                                                                      1,
                                                                      It.IsAny<string>(),
                                                                      It.Is<DateTime>((value) => value >= expectedNextSchedule)),
                                                    Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_When_ExceptionThrown_Then_NotificationUpdatedWithWaitingForRetry()
        {
            // Arrange
            var mock = new MockNotificationService();
            mock.SetupTransporters();
            var pendingNotifications = new INotificationData[]
            {
                new MockNotificationData()
                {
                    TransportType = mock.MockNotificationTransporter.Object.TransporterType
                }
            };

            var expectedNextSchedule = DateTime.UtcNow.AddSeconds(60);

            mock.MockNotificationRepository.Setup(x => x.GetPendingNotificationsAsync(It.Is<DateTime>(mock.ExpectedPendingNotificationsTimestampCheck), It.IsAny<int?>())).Returns(Task.FromResult<IEnumerable<INotificationData>>(pendingNotifications));
            mock.MockNotificationTransporter.Setup(x => x.SendAsync(pendingNotifications[0].NotificationType, pendingNotifications[0].Recipients, pendingNotifications[0].Content))
                                            .Callback(() => throw new SystemException());

            // Act
            var result = await mock.Service.ExecuteAsync();

            // Assert
            Assert.Equal(0, result);
            mock.MockNotificationRepository.Verify(x => x.GetPendingNotificationsAsync(It.Is<DateTime>(mock.ExpectedPendingNotificationsTimestampCheck), It.IsAny<int?>()), Times.Once);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(pendingNotifications[0].Id, 
                                                                      NotificationState.Processing,
                                                                      0,
                                                                      null,
                                                                      null), 
                                                    Times.Once);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(pendingNotifications[0].Id,
                                                                      NotificationState.WaitingForRetry,
                                                                      1,
                                                                      It.IsAny<string>(),
                                                                      It.Is<DateTime>((value) => value >= expectedNextSchedule)),
                                                    Times.Once);


            mock.MockNotificationTransporter.Verify(x => x.SendAsync(pendingNotifications[0].NotificationType,
                                                                     pendingNotifications[0].Recipients,
                                                                     pendingNotifications[0].Content),
                                                    Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_When_TransporterSendReportsErrors_Then_NotificationUpdatedWithWaitingForRetry()
        {
            // Arrange
            var mock = new MockNotificationService();
            mock.SetupTransporters();
            var pendingNotifications = new INotificationData[]
            {
                new MockNotificationData()
                {
                    TransportType = mock.MockNotificationTransporter.Object.TransporterType
                }
            };

            var expectedNextSchedule = DateTime.UtcNow.AddSeconds(60);

            mock.MockNotificationRepository.Setup(x => x.GetPendingNotificationsAsync(It.Is<DateTime>(mock.ExpectedPendingNotificationsTimestampCheck), It.IsAny<int?>())).Returns(Task.FromResult<IEnumerable<INotificationData>>(pendingNotifications));

            var transporterErrors = new string[] { "error" };
            mock.MockNotificationTransporter.Setup(x => x.SendAsync(pendingNotifications[0].NotificationType, pendingNotifications[0].Recipients, pendingNotifications[0].Content))
                                            .Returns(Task.FromResult<IEnumerable<string>>(transporterErrors));

            // Act
            var result = await mock.Service.ExecuteAsync();

            // Assert
            Assert.Equal(0, result);
            mock.MockNotificationRepository.Verify(x => x.GetPendingNotificationsAsync(It.Is<DateTime>(mock.ExpectedPendingNotificationsTimestampCheck), It.IsAny<int?>()), Times.Once);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(pendingNotifications[0].Id, 
                                                                      NotificationState.Processing,
                                                                      0,
                                                                      null,
                                                                      null), 
                                                    Times.Once);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(pendingNotifications[0].Id,
                                                                      NotificationState.WaitingForRetry,
                                                                      1,
                                                                      transporterErrors[0],
                                                                      It.Is<DateTime>((value) => value >= expectedNextSchedule)),
                                                    Times.Once);


            mock.MockNotificationTransporter.Verify(x => x.SendAsync(pendingNotifications[0].NotificationType,
                                                                     pendingNotifications[0].Recipients,
                                                                     pendingNotifications[0].Content),
                                                    Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_When_TransporterErrorsMaximumAmount_Then_NotificationUpdatedWithFailed()
        {
            // Arrange
            var mock = new MockNotificationService();
            mock.Options.MaximumRetryCount = 1;
            mock.SetupTransporters();
            var pendingNotifications = new INotificationData[]
            {
                new MockNotificationData()
                {
                    TransportType = mock.MockNotificationTransporter.Object.TransporterType,
                    RetryCount = 1
                }
            };

            mock.MockNotificationRepository.Setup(x => x.GetPendingNotificationsAsync(It.Is<DateTime>(mock.ExpectedPendingNotificationsTimestampCheck), It.IsAny<int?>())).Returns(Task.FromResult<IEnumerable<INotificationData>>(pendingNotifications));

            var transporterErrors = new string[] { "error" };
            mock.MockNotificationTransporter.Setup(x => x.SendAsync(pendingNotifications[0].NotificationType, pendingNotifications[0].Recipients, pendingNotifications[0].Content))
                                            .Returns(Task.FromResult<IEnumerable<string>>(transporterErrors));

            // Act
            var result = await mock.Service.ExecuteAsync();

            // Assert
            Assert.Equal(0, result);
            mock.MockNotificationRepository.Verify(x => x.GetPendingNotificationsAsync(It.Is<DateTime>(mock.ExpectedPendingNotificationsTimestampCheck), It.IsAny<int?>()), Times.Once);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(pendingNotifications[0].Id, 
                                                                      NotificationState.Processing,
                                                                      1,
                                                                      null,
                                                                      null), 
                                                    Times.Once);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(pendingNotifications[0].Id,
                                                                      NotificationState.Failed,
                                                                      1,
                                                                      transporterErrors[0],
                                                                      null),
                                                    Times.Once);


            mock.MockNotificationTransporter.Verify(x => x.SendAsync(pendingNotifications[0].NotificationType,
                                                                     pendingNotifications[0].Recipients,
                                                                     pendingNotifications[0].Content),
                                                    Times.Once);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ExecuteAsync_When_TransporterSendIsSuccessful_Then_NotificationUpdatedWithSuccessful(bool isErrorsCollectionNull)
        {
            // Arrange
            var mock = new MockNotificationService();
            mock.Options.MaximumRetryCount = 1;
            mock.SetupTransporters();
            var pendingNotifications = new INotificationData[]
            {
                new MockNotificationData()
                {
                    TransportType = mock.MockNotificationTransporter.Object.TransporterType
                }
            };

            mock.MockNotificationRepository.Setup(x => x.GetPendingNotificationsAsync(It.Is<DateTime>(mock.ExpectedPendingNotificationsTimestampCheck), It.IsAny<int?>())).Returns(Task.FromResult<IEnumerable<INotificationData>>(pendingNotifications));

            mock.MockNotificationTransporter.Setup(x => x.SendAsync(pendingNotifications[0].NotificationType, pendingNotifications[0].Recipients, pendingNotifications[0].Content))
                                            .Returns(Task.FromResult<IEnumerable<string>>(isErrorsCollectionNull ? null : new string[0]));

            // Act
            var result = await mock.Service.ExecuteAsync();

            // Assert
            Assert.Equal(1, result);
            mock.MockNotificationRepository.Verify(x => x.GetPendingNotificationsAsync(It.Is<DateTime>(mock.ExpectedPendingNotificationsTimestampCheck), It.IsAny<int?>()), Times.Once);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(pendingNotifications[0].Id, 
                                                                      NotificationState.Processing,
                                                                      0,
                                                                      null,
                                                                      null), 
                                                    Times.Once);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(pendingNotifications[0].Id,
                                                                      NotificationState.Successful,
                                                                      0,
                                                                      null,
                                                                      null),
                                                    Times.Once);


            mock.MockNotificationTransporter.Verify(x => x.SendAsync(pendingNotifications[0].NotificationType,
                                                                     pendingNotifications[0].Recipients,
                                                                     pendingNotifications[0].Content),
                                                    Times.Once);
        }

        #endregion

        #region ScheduleAndExecuteAsync

        [Fact]
        public async Task ScheduleAndExecuteAsync_When_ContentIsNull_Then_ArgumentNullExceptionThrown()
        {
            // Arrange
            var mock = new MockNotificationService();
            var recipient = new Recipient();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => mock.Service.ScheduleAndExecuteAsync(null, recipient));
        }

        [Fact]
        public async Task ScheduleAndExecuteAsync_When_RecipientIsNull_Then_ArgumentNullExceptionThrown()
        {
            // Arrange
            var mock = new MockNotificationService();
            var content = new NotificationContent();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => mock.Service.ScheduleAndExecuteAsync(content, null));
        }

        [Fact]
        public async Task ScheduleAndExecuteAsync_When_NotificationNotCreated_Then_SuccessfulReturned()
        {
            // Arrange
            var mock = new MockNotificationService();
            var recipient = new Recipient();
            var content = new NotificationContent();

            // Act
            var result = await mock.Service.ScheduleAndExecuteAsync(content, recipient);
        
            // Assert
            Assert.Equal(NotifyStatus.Successful, result);
        }

        [Fact]
        public async Task ScheduleAndExecuteAsync_When_NotificationCreated_NotificationNotSent_Then_ScheduledReturned()
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
                new MockNotificationData()
                {
                    TransportType = mock.MockNotificationTransporter.Object.TransporterType
                }
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

            var expectedNextSchedule = DateTime.UtcNow.AddSeconds(60);

            mock.MockNotificationTransporter.Setup(x => x.SendAsync(expectedAddedNotifications[0].NotificationType, 
                                                                    expectedAddedNotifications[0].Recipients, 
                                                                    expectedAddedNotifications[0].Content))
                                            .Callback(() => throw new SystemException());

            // Act
            var result = await mock.Service.ScheduleAndExecuteAsync(content, recipient);
        
            // Assert
            Assert.Equal(NotifyStatus.Scheduled, result);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(expectedAddedNotifications[0].Id, 
                                                                      NotificationState.Processing,
                                                                      0,
                                                                      null,
                                                                      null), 
                                                    Times.Once);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(expectedAddedNotifications[0].Id,
                                                                      NotificationState.WaitingForRetry,
                                                                      1,
                                                                      It.IsAny<string>(),
                                                                      It.Is<DateTime>((value) => value >= expectedNextSchedule)),
                                                    Times.Once);


            mock.MockNotificationTransporter.Verify(x => x.SendAsync(expectedAddedNotifications[0].NotificationType,
                                                                     expectedAddedNotifications[0].Recipients,
                                                                     expectedAddedNotifications[0].Content),
                                                    Times.Once);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ScheduleAndExecuteAsync_When_NotificationCreated_NotificationSent_Then_SuccessfulReturned(bool isErrorsCollectionNull)
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
                new MockNotificationData()
                {
                    TransportType = mock.MockNotificationTransporter.Object.TransporterType
                }
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

            mock.MockNotificationTransporter.Setup(x => x.SendAsync(expectedAddedNotifications[0].NotificationType, 
                                                                    expectedAddedNotifications[0].Recipients, 
                                                                    expectedAddedNotifications[0].Content))
                                            .Returns(Task.FromResult<IEnumerable<string>>(isErrorsCollectionNull ? null : new string[0]));

            // Act
            var result = await mock.Service.ScheduleAndExecuteAsync(content, recipient);
        
            // Assert
            Assert.Equal(NotifyStatus.Successful, result);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(expectedAddedNotifications[0].Id, 
                                                                      NotificationState.Processing,
                                                                      0,
                                                                      null,
                                                                      null), 
                                                    Times.Once);

            mock.MockNotificationRepository.Verify(x => x.UpdateAsync(expectedAddedNotifications[0].Id,
                                                                      NotificationState.Successful,
                                                                      0,
                                                                      null,
                                                                      null),
                                                    Times.Once);


            mock.MockNotificationTransporter.Verify(x => x.SendAsync(expectedAddedNotifications[0].NotificationType,
                                                                     expectedAddedNotifications[0].Recipients,
                                                                     expectedAddedNotifications[0].Content),
                                                    Times.Once);
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

        public class MockNotificationData : INotificationData
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

            public System.Linq.Expressions.Expression<Func<DateTime, bool>> ExpectedPendingNotificationsTimestampCheck { get; }

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

                // Our pending notifications should have a minimum timestamp in relation to our options.
                ExpectedPendingNotificationsTimestampCheck = (value) => value <= DateTime.UtcNow.AddSeconds(Options.PendingNotificationOffsetInSeconds * -1) &&
                                                                        value >= DateTime.UtcNow.AddSeconds((Options.PendingNotificationOffsetInSeconds + 5) * -1);

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

                MockNotificationTransportManager.Setup(x => x.Get(MockNotificationTransporter.Object.TransporterType)).Returns(MockNotificationTransporter.Object);
            }
        }
    }
}
