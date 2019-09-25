using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Services;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Handlers;

namespace SFA.DAS.Forecasting.Jobs.Application.UnitTests
{
    [TestFixture, Parallelizable]
    public class WhenHandlingEmployerLevyRefreshComplete
    {
        private IFixture Fixture => new Fixture();
        private Mock<ILevyForecastService> _levyForecastServiceMock;
        private LevyCompleteTriggerHandler _sut;
        private RefreshEmployerLevyDataCompletedEvent _event;

        [SetUp]
        public void SetUp()
        {
            _levyForecastServiceMock = new Mock<ILevyForecastService>();
            _sut = new LevyCompleteTriggerHandler(_levyForecastServiceMock.Object);

            _event = Fixture
                .Build<RefreshEmployerLevyDataCompletedEvent>()
                .With(e => e.LevyImported, true)
                .Create();
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
            _levyForecastServiceMock.Verify(mock => mock.TriggerLevyForecast(It.IsAny<short>(), It.IsAny<string>(), It.IsAny<long>()), Times.Never);
        }

        [Test]
        [Category("UnitTest")]
        public async Task Should_Trigger_Levy_Forecast()
        {
            // Arrange

            // Act
            await _sut.Handle(_event);

            // Assert
            _levyForecastServiceMock.Verify(mock => mock.TriggerLevyForecast(It.IsAny<short>(), It.IsAny<string>(), It.IsAny<long>()), Times.Once);
        }

        [Test]
        [Category("UnitTest")]
        [TestCase(4, 2019, 1)]
        [TestCase(5, 2019, 2)]
        [TestCase(1, 2020, 10)]
        [TestCase(4, 2020, 1)]

        public async Task If_No_PeriodMonth_Should_Calculate_Today_Month(int currentMonth, int currentYear, short expectedPeriodMonth)
        {
            // Arrange
            _event.PeriodMonth = 0;
            _event.PeriodYear = string.Empty;
            _event.Created = new DateTime(currentYear, currentMonth, 6);
            short actualPeriodMonth = 0;

            _levyForecastServiceMock
                .Setup(mock => mock.TriggerLevyForecast(It.IsAny<short>(), It.IsAny<string>(), It.IsAny<long>()))
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
        [TestCase(4, 2019, "19-20")]
        [TestCase(5, 2019, "19-20")]
        [TestCase(1, 2020, "19-20")]
        [TestCase(4, 2020, "20-21")]

        public async Task If_No_PeriodYear_Should_Calculate_Today_Year(int currentMonth, int currentYear, string expectedPeriodYear)
        {
            // Arrange
            _event.PeriodMonth = 0;
            _event.PeriodYear = string.Empty;
            _event.Created = new DateTime(currentYear, currentMonth, 6);
            string actualPeriodYear = string.Empty;

            _levyForecastServiceMock
                .Setup(mock => mock.TriggerLevyForecast(It.IsAny<short>(), It.IsAny<string>(), It.IsAny<long>()))
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
}