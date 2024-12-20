using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Services;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Handlers;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Models;

namespace SFA.DAS.Forecasting.Jobs.Application.UnitTests.Handlers;

public class WhenHandlingPaymentDataRefreshComplete
{
    private readonly IFixture _fixture = new Fixture();
    private PaymentCompleteTriggerHandler _sut;
    private RefreshPaymentDataCompletedEvent _event;
    private Mock<IPaymentForecastService> _paymentForecastServiceMock;

    [SetUp]
    public void SetUp()
    {
        _paymentForecastServiceMock = new Mock<IPaymentForecastService>();
        _sut = new PaymentCompleteTriggerHandler(_paymentForecastServiceMock.Object);
        _event = _fixture
            .Build<RefreshPaymentDataCompletedEvent>()
            .With(e => e.PaymentsProcessed, true)
            .With(e => e.PeriodEnd, "1819-R10")
            .Create();
    }

    [Test]
    [Category("UnitTest")]
    public async Task Should_Trigger_Payment_Forecast()
    {
        // Arrange

        // Act
        await _sut.Handle(_event);

        // Assert
        _paymentForecastServiceMock.Verify(mock => mock.Trigger(It.IsAny<short>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    [Category("UnitTest")]
    [TestCase("1920-R01", 8)]
    [TestCase("1819-R12", 7)]
    [TestCase("9899-R05", 12)]
    [TestCase("0001-R06", 1)]
    public async Task Should_Calculate_Period_Month_From_PeriodEnd(string periodEnd, short expectedMonth)
    {
        // Arrange 
        _event.PeriodEnd = periodEnd;
        short actualPeriodMonth = 0;
        
        _paymentForecastServiceMock
            .Setup(mock => mock.Trigger(It.IsAny<short>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>()))
            .Callback((short periodMonth, int periodYear, string callbackPeriodEnd, long accountId) =>
            {
                actualPeriodMonth = periodMonth;
            })
            .Returns(Task.CompletedTask);

        //Act
        await _sut.Handle(_event);

        //Assert
        actualPeriodMonth.Should().Be(expectedMonth);
    }

    [Test]
    [Category("UnitTest")]
    [TestCase("1920-R01", 2019)]
    [TestCase("1819-R12", 2019)]
    [TestCase("9899-R05", 2098)]
    [TestCase("0001-R06", 2001)]
    [TestCase("2021-R06", 2021)]
    public async Task Should_Calculate_Period_Year_From_PeriodEnd(string periodEnd, int expectedYear)
    {
        // Arrange 
        _event.PeriodEnd = periodEnd;
        var actualPeriodYear = 0;

        _paymentForecastServiceMock
            .Setup(mock => mock.Trigger(It.IsAny<short>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>()))
            .Callback((short periodMonth, int periodYear, string callbackPeriodEnd, long accountId) =>
            {
                actualPeriodYear = periodYear;
            })
            .Returns(Task.CompletedTask);

        //Act
        await _sut.Handle(_event);

        //Assert
        actualPeriodYear.Should().Be(expectedYear);
    }
}