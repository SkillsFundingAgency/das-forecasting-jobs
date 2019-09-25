using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
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
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Handlers;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Models;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Services;

namespace SFA.DAS.Forecasting.Jobs.Application.UnitTests.Services
{
    public class WhenTiggeringPaymentForecast
    {
        private IFixture Fixture => new Fixture();
        private ForecastingJobsConfiguration _config;
        private Mock<IHttpFunctionClient<PaymentDataCompleteTrigger>> _httpClientMock;
        private PaymentForecastService _sut;
        private RefreshPaymentDataCompletedEvent _event;
        private Mock<ILogger<PaymentForecastService>> _loggerMock;
        private Mock<IEncodingService> _encodingServiceMock;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _config = new ForecastingJobsConfiguration {PaymentPreLoadHttpFunctionBaseUrl = "FunctionBaseUrl"};
        }

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<PaymentForecastService>>();
            _httpClientMock = new Mock<IHttpFunctionClient<PaymentDataCompleteTrigger>>();
            _encodingServiceMock = new Mock<IEncodingService>();
            _sut = new PaymentForecastService(Options.Create(_config), _httpClientMock.Object, _encodingServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        [Category("UnitTest")]
        public async Task Should_Trigger_Payment_Forecast()
        {
            // Arrange
            _httpClientMock
                .Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<PaymentDataCompleteTrigger>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            // Act
            await _sut.TriggerPaymentForecast(1, 19, "18-19R10", 1);

            // Assert
            _httpClientMock.Verify(mock => mock.PostAsync(It.Is<string>(x => x == _config.PaymentPreLoadHttpFunctionBaseUrl), It.IsAny<PaymentDataCompleteTrigger>()), Times.Once);
        }

        [Test]
        [Category("UnitTest")]
        public void If_Trigger_Errors_Should_Log_Error()
        {
            // Arrange
            _httpClientMock
                .Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<PaymentDataCompleteTrigger>()))
                .ThrowsAsync(new Exception("Its Broken"));

            // Act

            // Assert
            Assert.ThrowsAsync<Exception>(() => _sut.TriggerPaymentForecast(1, 19, "18-19R10", 1));
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
            _httpClientMock
                .Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<PaymentDataCompleteTrigger>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = statusCode });

            // Act
            await _sut.TriggerPaymentForecast(1, 19, "18-19R10", 1);

            // Assert
            _loggerMock.Verify(
               x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(),
                   It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
    }
}