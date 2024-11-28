using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;

namespace SFA.DAS.Forecasting.Triggers.Functions;

public class TestFunction(ILogger<TestFunction> log, ILevyCompleteTriggerHandler handler)
{
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "TestHttpClientFunction")]HttpRequestMessage req)
    {
        log.LogInformation("TestHttpClientFunction function executed.");

        var convertedMessage = new RefreshEmployerLevyDataCompletedEvent
        {
            AccountId = 1,
            Created = DateTime.UtcNow,
            // Allow forecasting to be triggered for Expiry event
            LevyImported = true
        };

        await handler.Handle(convertedMessage);
    }
}