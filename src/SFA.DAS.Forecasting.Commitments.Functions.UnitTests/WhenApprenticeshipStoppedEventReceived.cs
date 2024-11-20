using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Mapper;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Forecasting.Commitments.Functions.Functions;

namespace SFA.DAS.Forecasting.Commitments.Functions.UnitTests;

[TestFixture]
public class WhenApprenticeshipStoppedEventReceived
{
    [Test]
    public async Task Then_Message_Will_Be_Handled()
    {
        //Arrange
        var fixture = new ApprenticeshipStoppedEventTestsFixture();

        //Act
        await fixture.Run();

        //Assert
        fixture.AssertHandler();
    }
}

public class ApprenticeshipStoppedEventTestsFixture
{
    public Mock<IApprenticeshipStoppedEventHandler> MockpprenticeshipStoppedEventHandler { get; set; }
    public Mock<ILogger<ApprenticeshipStoppedFunction>> MockLogger { get; set; }
    public Fixture Fixture { get; set; }
    public ApprenticeshipStoppedEvent ApprenticeshipStoppedEvent { get; set; }
    public ApprenticeshipStoppedFunction Sut { get; set; }
    public ApprenticeshipStoppedEventTestsFixture()
    {
        MockpprenticeshipStoppedEventHandler = new Mock<IApprenticeshipStoppedEventHandler>();
        MockLogger = new Mock<ILogger<ApprenticeshipStoppedFunction>>();
        Fixture = new Fixture();

        ApprenticeshipStoppedEvent = Fixture.Create<ApprenticeshipStoppedEvent>();
        Sut = new ApprenticeshipStoppedFunction(MockpprenticeshipStoppedEventHandler.Object, MockLogger.Object);
    }

    public async Task Run() => await Sut.Handle(ApprenticeshipStoppedEvent, Mock.Of<IMessageHandlerContext>());

    internal void AssertHandler()
    {
        MockLogger.VerifyLogging(LogLevel.Information);
        MockpprenticeshipStoppedEventHandler.Verify(s => s.Handle(It.Is<ApprenticeshipStoppedEvent>(c => c.ApprenticeshipId.Equals(ApprenticeshipStoppedEvent.ApprenticeshipId))), Times.Once);
    }
}