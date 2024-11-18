using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;
using System;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Forecasting.Triggers.Functions;

namespace SFA.DAS.Forecasting.Functions.Triggers.UnitTests;

[TestFixture, Parallelizable]
public class WhenAccountFundsExpiredTriggered
{
    [Test]
    [Category("UnitTest")]
    public async Task Then_Message_Will_Be_Handled()
    {
        //Arrange
        var createdDate = DateTime.Now;
        var handler = new Mock<ILevyCompleteTriggerHandler>();
        var message = new AccountFundsExpiredEvent { AccountId = 123, Created = createdDate };
        var function = new HandleAccountFundsExpiredEvent(Mock.Of<ILogger<HandleAccountFundsExpiredEvent>>(), handler.Object);

        //Act
        await function.Handle(message, Mock.Of<IMessageHandlerContext>());

        //Assert
        handler.Verify(
            s => s.Handle(It.Is<RefreshEmployerLevyDataCompletedEvent>(c => c.AccountId.Equals(message.AccountId) && c.Created == message.Created)),
            Times.Once);
    }
}