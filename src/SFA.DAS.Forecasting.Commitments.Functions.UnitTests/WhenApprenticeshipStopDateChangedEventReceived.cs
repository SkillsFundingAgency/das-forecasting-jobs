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
    public class WhenApprenticeshipStopDateChangedEventReceived
    {
        [Test]
        public async Task Then_Message_Will_Be_Handled()
        {
            //Arrange
            var fixture = new ApprenticeshipStopDateChangedEventReceivedFixture();

            //Act
            await fixture.Run();

            //Assert
            fixture.AssertHandler();
        }
    }

    public class ApprenticeshipStopDateChangedEventReceivedFixture
    {
        public Mock<ILogger<ApprenticeshipStopDateChangedFunction>> MockLogger { get; set; }
        public Mock<IApprenticeshipStopDateChangedEventHandler> MockapprenticeshipUpdatedApprovedEventHandler { get; set; }
        public Fixture Fixture { get; set; }
        public ApprenticeshipStopDateChangedFunction Sut { get; set; }

        public ApprenticeshipStopDateChangedEvent ApprenticeshipStopDateChangedEvent { get; set; }

        public ApprenticeshipStopDateChangedEventReceivedFixture()
        {
            MockLogger = new Mock<ILogger<ApprenticeshipStopDateChangedFunction>>();
            MockapprenticeshipUpdatedApprovedEventHandler = new Mock<IApprenticeshipStopDateChangedEventHandler>();
            Fixture = new Fixture();

            ApprenticeshipStopDateChangedEvent = Fixture.Create<ApprenticeshipStopDateChangedEvent>();

            Sut = new ApprenticeshipStopDateChangedFunction(MockapprenticeshipUpdatedApprovedEventHandler.Object, MockLogger.Object);
        }

        public async Task Run()
        {
            await Sut.Run(ApprenticeshipStopDateChangedEvent);
        }

        internal void AssertHandler()
        {
            MockLogger.VerifyLogging(LogLevel.Information);
            MockapprenticeshipUpdatedApprovedEventHandler.Verify(s => s.Handle(It.Is<ApprenticeshipStopDateChangedEvent>(c => c.ApprenticeshipId.Equals(ApprenticeshipStopDateChangedEvent.ApprenticeshipId))), Times.Once);
        }
    }
}