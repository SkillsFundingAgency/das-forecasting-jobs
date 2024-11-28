using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;

namespace SFA.DAS.Forecasting.Commitments.Functions.Functions;

public class TestFunction(ILogger<TestFunction> logger, IApprenticeshipCompletedEventHandler handler)
{
    [Function("TestCosmosDbFunction")]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "TestCosmosDbFunction")] HttpRequestMessage req)
    {
        logger.LogInformation("TestCosmosDbFunction function executed.");

        await handler.Handle(new ApprenticeshipCompletedEvent
        {
            ApprenticeshipId = 100,
            CompletionDate = DateTime.UtcNow
        });
    }
}