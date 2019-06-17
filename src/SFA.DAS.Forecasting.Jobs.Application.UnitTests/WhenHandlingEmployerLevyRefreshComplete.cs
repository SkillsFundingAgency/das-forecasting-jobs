using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

namespace SFA.DAS.Forecasting.Jobs.Application.UnitTests
{
    public class WhenHandlingEmployerLevyRefreshComplete
    {
        private ForecastingJobsConfiguration _config;
        private Mock<IHttpFunctionClient<AccountLevyCompleteTrigger>> _httpClientMock;
        private LevyCompleteTriggerHandler _sut;
        private RefreshEmployerLevyDataCompletedEvent _event;
        private Mock<ILogger> _loggerMock;
        private Mock<IEncodingService> _encodingServiceMock;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _config = new ForecastingJobsConfiguration {LevyDeclarationPreLoadHttpFunctionBaseUrl = "FunctionBaseUrl"};
            _event = new RefreshEmployerLevyDataCompletedEvent();
        }

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger>();
            _httpClientMock = new Mock<IHttpFunctionClient<AccountLevyCompleteTrigger>>();
            _encodingServiceMock = new Mock<IEncodingService>();
            _sut = new LevyCompleteTriggerHandler(Options.Create(_config), _httpClientMock.Object, _encodingServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        [Category("UnitTest")]
        public async Task Should_Trigger_Levy_Forecast()
        {
            // Arrange
            _httpClientMock.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<AccountLevyCompleteTrigger>())).ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            // Act
            await _sut.Handle(_event);

            // Assert
            _httpClientMock.Verify(mock => mock.PostAsync(It.Is<string>(x => x == _config.LevyDeclarationPreLoadHttpFunctionBaseUrl), It.IsAny<AccountLevyCompleteTrigger>()), Times.Once);
        }

        [Test]
        [Category("UnitTest")]
        public async Task If_Trigger_Errors_Should_Log_Error()
        {
            // Arrange
            _httpClientMock.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<AccountLevyCompleteTrigger>())).ThrowsAsync(new Exception("Its Broken"));

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
            _httpClientMock.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<AccountLevyCompleteTrigger>())).ReturnsAsync(new HttpResponseMessage { StatusCode = statusCode });

            // Act
            await _sut.Handle(_event);

            // Assert
            _loggerMock.Verify(
               x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(),
                   It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
    }
}