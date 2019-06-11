using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Triggers;

namespace Tests
{
    [TestFixture]
    public class Tests
    {

        [Test]
        [Category("UnitTest")]
        public async Task Test1()
        {
            //Arrange
            var handler = new Mock<ILevyCompleteTriggerHandler>();
            var message = new RefreshEmployerLevyDataCompletedEvent { AccountId = 123 };

            //Act
            await HandleRefreshEmployerLevyDataCompleted.Run(message, handler.Object, Mock.Of<ILogger>());

            //Assert
            handler.Verify(s => s.Handle(It.Is<RefreshEmployerLevyDataCompletedEvent>(c => c.AccountId.Equals(message.AccountId))), Times.Once);
        }
    }
}