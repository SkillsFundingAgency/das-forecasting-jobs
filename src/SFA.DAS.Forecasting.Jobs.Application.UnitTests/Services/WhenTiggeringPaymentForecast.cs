using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Infrastructure;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Models;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;

namespace SFA.DAS.Forecasting.Jobs.Application.UnitTests.Services;

public class WhenTiggeringPaymentForecast
{
    private ForecastingJobsConfiguration _config;
    private Mock<IHttpFunctionClient<PaymentDataCompleteTrigger>> _httpClientMock;
    private PaymentForecastService _sut;
    private Mock<ILogger<PaymentForecastService>> _loggerMock;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _config = new ForecastingJobsConfiguration { PaymentPreLoadHttpFunctionBaseUrl = "FunctionBaseUrl" };
    }

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<PaymentForecastService>>();
        _httpClientMock = new Mock<IHttpFunctionClient<PaymentDataCompleteTrigger>>();
        _sut = new PaymentForecastService(Options.Create(_config), _httpClientMock.Object, _loggerMock.Object);
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
        await _sut.Trigger(1, 19, "18-19R10", 1);

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
        var action = () => _sut.Trigger(1, 19, "18-19R10", 1);
        action.Should().ThrowAsync<Exception>();
        
        _loggerMock.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.Is<Exception>(e => e.Message == "Its Broken"),
                (Func<object, Exception, string>)It.IsAny<object>()), Times.Once);
    }

    [Test]
    [Category("UnitTest")]
    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.ServiceUnavailable)]
    public void If_Http_Call_Unsuccesful_Should_Log_Error(HttpStatusCode statusCode)
    {
        // Arrange
        _httpClientMock
            .Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<PaymentDataCompleteTrigger>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = statusCode });

        // Act
        var action = () => _sut.Trigger(1, 19, "18-19R10", 1);
        action.Should().ThrowAsync<Exception>();

        // Assert
        _loggerMock.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                (Func<object, Exception, string>)It.IsAny<object>()), Times.AtLeastOnce());
    }
}