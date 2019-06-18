using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Encoding;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Infrastructure;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Handlers;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Models;

namespace SFA.DAS.Forecasting.Jobs.Application.UnitTests
{
    public class WhenHandlingPaymentDataRefreshComplete
    {
        private ForecastingJobsConfiguration _config;
        private Mock<IHttpFunctionClient<PaymentDataCompleteTrigger>> _httpClientMock;
        private PaymentCompleteTriggerHandler _sut;
        private RefreshPaymentDataCompletedEvent _event;
        private Mock<ILogger<PaymentCompleteTriggerHandler>> _loggerMock;
        private Mock<IEncodingService> _encodingServiceMock;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _config = new ForecastingJobsConfiguration {PaymentPreLoadHttpFunctionBaseUrl = "FunctionBaseUrl"};
            _event = new RefreshPaymentDataCompletedEvent
            {
                AccountId = 1,
                PeriodEnd = "1920-R01"
            };
        }

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<PaymentCompleteTriggerHandler>>();
            _httpClientMock = new Mock<IHttpFunctionClient<PaymentDataCompleteTrigger>>();
            _encodingServiceMock = new Mock<IEncodingService>();
            _sut = new PaymentCompleteTriggerHandler(Options.Create(_config), _httpClientMock.Object, _encodingServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        [Category("UnitTest")]
        public async Task Should_Trigger_Levy_Forecast()
        {
            // Arrange
            _httpClientMock.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<PaymentDataCompleteTrigger>())).ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            // Act
            await _sut.Handle(_event);

            // Assert
            _httpClientMock.Verify(mock => mock.PostAsync(It.Is<string>(x => x == _config.PaymentPreLoadHttpFunctionBaseUrl), It.IsAny<PaymentDataCompleteTrigger>()), Times.Once);
        }

        [Test]
        [Category("UnitTest")]
        public async Task If_Trigger_Errors_Should_Log_Error()
        {
            // Arrange
            _httpClientMock.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<PaymentDataCompleteTrigger>())).ThrowsAsync(new Exception("Its Broken"));

            // Act

            // Assert
            Assert.ThrowsAsync<Exception>(() => _sut.Handle(_event));
            _loggerMock.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.Is<Exception>(e => e.Message == "Its Broken"),
                    It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }

        [Test]
        [Category("UnitTest")]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        public async Task If_Http_Call_Unsuccesful_Should_Log_Error(HttpStatusCode statusCode)
        {
            // Arrange
            _httpClientMock.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<PaymentDataCompleteTrigger>())).ReturnsAsync(new HttpResponseMessage { StatusCode = statusCode });

            // Act
            await _sut.Handle(_event);

            // Assert
            _loggerMock.Verify(
               x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(),
                   It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }

        [Test]
        [Category("UnitTest")]
        [TestCase("1920-R01",2019,8)]
        [TestCase("1718-R12",2017,7)]
        [TestCase("9899-R05",2098,12)]
        [TestCase("0001-R06",2000,1)]
        public async Task ShouldTranslatePeriodEndCorrectly(string periodEnd,int expectedYear,int expectedMonth)
        {
            // Arrange 
            _event.PeriodEnd = periodEnd;
            PaymentDataCompleteTrigger res = new PaymentDataCompleteTrigger();
            _httpClientMock.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<PaymentDataCompleteTrigger>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK})
                .Callback((string url, PaymentDataCompleteTrigger trigger) =>
                {
                    res = trigger;
                });

            //Act
            await _sut.Handle(_event);

            //Assert
            res.PeriodMonth.Should().Be(expectedMonth);
            res.PeriodYear.Should().Be(expectedYear);
        }
    }
}