using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Triggers;

namespace SFA.DAS.Forecasting.Functions.Triggers.UnitTests
{
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

            //Act
            await HandleRefreshPaymentDataCompletedEvent.Run(message, handler.Object, Mock.Of<ILogger>());

            //Assert
            handler.Verify(s => s.Handle(It.Is<RefreshPaymentDataCompletedEvent>(c => c.AccountId.Equals(message.AccountId))), Times.Once);
        }
    }
}