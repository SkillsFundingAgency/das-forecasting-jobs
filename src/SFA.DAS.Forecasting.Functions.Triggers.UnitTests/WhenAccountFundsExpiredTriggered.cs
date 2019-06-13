using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Triggers;

namespace SFA.DAS.Forecasting.Functions.Triggers.UnitTests
{
    [TestFixture]
    public class WhenAccountFundsExpiredTriggered
    {

        [Test]
        [Category("UnitTest")]
        public async Task Then_Message_Will_Be_Handled()
        {
            //Arrange
            var handler = new Mock<ILevyCompleteTriggerHandler>();
            var message = new AccountFundsExpiredEvent { AccountId = 123 };

            //Act
            await HandleAccountFundsExpiredEvent.Run(message, handler.Object, Mock.Of<ILogger>());

            //Assert
            handler.Verify(s => s.Handle(It.Is<RefreshEmployerLevyDataCompletedEvent>(c => c.AccountId.Equals(message.AccountId))), Times.Once);
        }

        [Test]
        [Category("UnitTest")]
        [TestCase("01/01/2019","19/20")]
        [TestCase("01/01/2000","00/01")]
        [TestCase("01/01/2099","99/00")]
        public void Then_Period_Year_Is_Properly_Created(DateTime date, string expectedPeriodYear)
        {
            //Act
            var res = HandleAccountFundsExpiredEvent.ConvertDateToPeriodYear(date);

            //Assert
            res.Should().Be(expectedPeriodYear);
        }
    }
}