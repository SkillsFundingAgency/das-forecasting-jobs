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
    public class WhenApprenticeshipUpdatedApprovedEventReceived
    {
        [Test]
        public async Task Then_Message_Will_Be_Handled()
        {
            //Arrange
            var fixture = new ApprenticeshipUpdatedApprovedEventReceivedFixture();

            //Act
            await fixture.Run();

            //Assert
            fixture.AssertHandler();
        }
    }

    public class ApprenticeshipUpdatedApprovedEventReceivedFixture
    {
        public Mock<ILogger<ApprenticeshipUpdatedApprovedFunction>> MockLogger { get; set; }
        public Mock<IApprenticeshipUpdatedApprovedEventHandler> MockapprenticeshipUpdatedApprovedEventHandler { get; set; }
        public Fixture Fixture { get; set; }
        public ApprenticeshipUpdatedApprovedFunction Sut { get; set; }

        public ApprenticeshipUpdatedApprovedEvent ApprenticeshipUpdatedApprovedEvent { get; set; }

        public ApprenticeshipUpdatedApprovedEventReceivedFixture()
        {
            MockLogger = new Mock<ILogger<ApprenticeshipUpdatedApprovedFunction>>();
            MockapprenticeshipUpdatedApprovedEventHandler = new Mock<IApprenticeshipUpdatedApprovedEventHandler>();
            Fixture = new Fixture();

            ApprenticeshipUpdatedApprovedEvent = Fixture.Create<ApprenticeshipUpdatedApprovedEvent>();

            Sut = new ApprenticeshipUpdatedApprovedFunction(MockapprenticeshipUpdatedApprovedEventHandler.Object, MockLogger.Object);
        }

        public async Task Run()
        {
            await Sut.Run(ApprenticeshipUpdatedApprovedEvent);
        }

        internal void AssertHandler()
        {
            MockLogger.VerifyLogging(LogLevel.Information);
            MockapprenticeshipUpdatedApprovedEventHandler.Verify(s => s.Handle(It.Is<ApprenticeshipUpdatedApprovedEvent>(c => c.ApprenticeshipId.Equals(ApprenticeshipUpdatedApprovedEvent.ApprenticeshipId))), Times.Once);
        }
    }
}