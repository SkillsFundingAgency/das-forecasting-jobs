using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace SFA.DAS.Forecasting.Commitments.Functions.UnitTests;

public static class VerifyLog
{
    public static Mock<ILogger<T>> VerifyLogging<T>(this Mock<ILogger<T>> logger, LogLevel level, Times? times = null)
    {
        times ??= Times.AtLeastOnce();

        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), (Times)times);

        return logger;
    }
}