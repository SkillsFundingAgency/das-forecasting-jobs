using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Triggers;
using System.Threading.Tasks;
using NServiceBus;
using ILogger = Castle.Core.Logging.ILogger;

namespace SFA.DAS.Forecasting.Functions.Triggers.UnitTests;

[TestFixture]
public class WhenRefreshPaymentDataCompletedTriggered
{
    [Test]
    [Category("UnitTest")]
    public async Task Then_Message_Will_Be_Handled()
    {
        //Arrange
        var handler = new Mock<IRefreshPaymentDataCompletedTriggerHandler>();
        var message = new RefreshPaymentDataCompletedEvent { AccountId = 123 };
        var function = new HandleRefreshPaymentDataCompletedEvent(Mock.Of<ILogger<HandleRefreshPaymentDataCompletedEvent>>(), handler.Object);

        //Act
        await function.Handle(message, Mock.Of<IMessageHandlerContext>());

        //Assert
        handler.Verify(s => s.Handle(It.Is<RefreshPaymentDataCompletedEvent>(c => c.AccountId.Equals(message.AccountId))), Times.Once);
    }
}