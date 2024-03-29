using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.UnitTests;

[TestFixture]
public class WhenApprenticeshipCompletedEventReceived
{
    [Test]
    public async Task Then_Message_Will_Be_Handled()
    {
        //Arrange
        var fixture = new ApprenticeshipCompletedEventTestsFixture();

        //Act
        await fixture.Run();

        //Assert
        fixture.AssertHandler();
    }
}

public class ApprenticeshipCompletedEventTestsFixture
{
    public Mock<ILogger<ApprenticeshipCompletedFunction>> MockLogger { get; set; }
    public Mock<IApprenticeshipCompletedEventHandler> MockpprenticeshipCompletedEventHandler { get; set; }
    public Fixture Fixture { get; set; }
    public ApprenticeshipCompletedFunction Sut { get; set; }

    public ApprenticeshipCompletedEvent ApprenticeshipCompletedEvent { get; set; }

    public ApprenticeshipCompletedEventTestsFixture()
    {
        MockLogger = new Mock<ILogger<ApprenticeshipCompletedFunction>>();
        MockpprenticeshipCompletedEventHandler = new Mock<IApprenticeshipCompletedEventHandler>();
        Fixture = new Fixture();

        ApprenticeshipCompletedEvent = Fixture.Create<ApprenticeshipCompletedEvent>();

        Sut = new ApprenticeshipCompletedFunction(MockpprenticeshipCompletedEventHandler.Object, MockLogger.Object);
    }

    public async Task Run()
    {
        await Sut.Run(ApprenticeshipCompletedEvent);
    }

    internal void AssertHandler()
    {
        MockLogger.VerifyLogging(LogLevel.Information);
        MockpprenticeshipCompletedEventHandler.Verify(s => s.Handle(It.Is<ApprenticeshipCompletedEvent>(c => c.ApprenticeshipId.Equals(ApprenticeshipCompletedEvent.ApprenticeshipId))), Times.Once);
    }
}