using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.UnitTests
{
    [TestFixture]
    public class WhenApprenticeshipCompletionDateUpdatedEventReceived
    {
        [Test]
        public async Task Then_Message_Will_Be_Handled()
        {
            //Arrange
            var fixture = new ApprenticeshipCompletionDateUpdatedEventReceivedFixture();

            //Act
            await fixture.Run();

            //Assert
            fixture.AssertHandler();
        }
    }

    public class ApprenticeshipCompletionDateUpdatedEventReceivedFixture
    {
        public Mock<ILogger<ApprenticeshipCompletionDateUpdatedFunction>> MockLogger { get; set; }
        public Mock<IApprenticeshipCompletionDateUpdatedEventHandler> MockpprenticeshipCompletedEventHandler { get; set; }
        public Fixture Fixture { get; set; }
        public ApprenticeshipCompletionDateUpdatedFunction Sut { get; set; }

        public ApprenticeshipCompletionDateUpdatedEvent ApprenticeshipCompletionDateUpdatedEvent { get; set; }

        public ApprenticeshipCompletionDateUpdatedEventReceivedFixture()
        {
            MockLogger = new Mock<ILogger<ApprenticeshipCompletionDateUpdatedFunction>>();
            MockpprenticeshipCompletedEventHandler = new Mock<IApprenticeshipCompletionDateUpdatedEventHandler>();
            Fixture = new Fixture();

            ApprenticeshipCompletionDateUpdatedEvent = Fixture.Create<ApprenticeshipCompletionDateUpdatedEvent>();

            Sut = new ApprenticeshipCompletionDateUpdatedFunction(MockpprenticeshipCompletedEventHandler.Object, MockLogger.Object);
        }

        public async Task Run()
        {
            await Sut.Run(ApprenticeshipCompletionDateUpdatedEvent);
        }

        internal void AssertHandler()
        {
            MockLogger.VerifyLogging(LogLevel.Information);
            MockpprenticeshipCompletedEventHandler.Verify(s => s.Handle(It.Is<ApprenticeshipCompletionDateUpdatedEvent>(c => c.ApprenticeshipId.Equals(ApprenticeshipCompletionDateUpdatedEvent.ApprenticeshipId))), Times.Once);
        }
    }
}