using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Services;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Handlers;

namespace SFA.DAS.Forecasting.Jobs.Application.UnitTests.Handlers;

[TestFixture, Parallelizable]
public class WhenHandlingEmployerLevyRefreshComplete
{
    private Mock<ILevyForecastService> _levyForecastServiceMock;
    private LevyCompleteTriggerHandler _sut;
    private RefreshEmployerLevyDataCompletedEvent _event;

    [SetUp]
    public void SetUp()
    {
        _event = new RefreshEmployerLevyDataCompletedEvent
        {
            AccountId = 777,
            PeriodMonth = 9,
            PeriodYear = "18-19",
            LevyImported = true
        };

        _levyForecastServiceMock = new Mock<ILevyForecastService>();
        _sut = new LevyCompleteTriggerHandler(_levyForecastServiceMock.Object);
    }

    [Test]
    [Category("UnitTest")]
    public async Task If_No_Levy_Calculated_Should_Not_Trigger_Forecast()
    {
        // Arrange
        _event.LevyImported = false;

        // Act
        await _sut.Handle(_event);

        // Assert
        _levyForecastServiceMock.Verify(mock => mock.Trigger(It.IsAny<short>(), It.IsAny<string>(), It.IsAny<long>()), Times.Never);
    }

    [Test]
    [Category("UnitTest")]
    public async Task Should_Trigger_Levy_Forecast()
    {
        // Arrange

        // Act
        await _sut.Handle(_event);

        // Assert
        _levyForecastServiceMock.Verify(mock => mock.Trigger(_event.PeriodMonth, _event.PeriodYear, _event.AccountId), Times.Once);
    }

    [Test]
    [Category("UnitTest")]
    [TestCase(4, 2019, 12)]
    [TestCase(5, 2019, 1)]
    [TestCase(1, 2020, 9)]
    [TestCase(4, 2020, 12)]

    public async Task If_No_PeriodMonth_Should_Calculate_Today_Month(int currentMonth, int currentYear, short expectedPeriodMonth)
    {
        // Arrange
        _event.PeriodMonth = 0;
        _event.PeriodYear = string.Empty;
        _event.Created = new DateTime(currentYear, currentMonth, 6);
        short actualPeriodMonth = 0;

        _levyForecastServiceMock
            .Setup(mock => mock.Trigger(It.IsAny<short>(), It.IsAny<string>(), It.IsAny<long>()))
            .Callback((short periodMonth, string periodYear, long accountId) =>
            {
                actualPeriodMonth = periodMonth;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(_event);

        // Assert
        actualPeriodMonth.Should().Be(expectedPeriodMonth);
    }

    [Test]
    [Category("UnitTest")]
    [TestCase(4, 2019, "18-19")]
    [TestCase(5, 2019, "19-20")]
    [TestCase(1, 2020, "19-20")]
    [TestCase(4, 2020, "19-20")]

    public async Task If_No_PeriodYear_Should_Calculate_Today_Year(int currentMonth, int currentYear, string expectedPeriodYear)
    {
        // Arrange
        _event.PeriodMonth = 0;
        _event.PeriodYear = string.Empty;
        _event.Created = new DateTime(currentYear, currentMonth, 6);
        var actualPeriodYear = string.Empty;

        _levyForecastServiceMock
            .Setup(mock => mock.Trigger(It.IsAny<short>(), It.IsAny<string>(), It.IsAny<long>()))
            .Callback((short periodMonth, string periodYear, long accountId) =>
            {
                actualPeriodYear = periodYear;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(_event);

        // Assert
        actualPeriodYear.Should().Be(expectedPeriodYear);
    }
}