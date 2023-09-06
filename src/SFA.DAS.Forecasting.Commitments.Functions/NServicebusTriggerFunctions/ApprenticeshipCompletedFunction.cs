using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions;

public class ApprenticeshipCompletedFunction
{
    private readonly IApprenticeshipCompletedEventHandler _apprenticeshipCompletedEventHandler;
    private readonly ILogger<ApprenticeshipCompletedFunction> _logger;

    public ApprenticeshipCompletedFunction(
        IApprenticeshipCompletedEventHandler apprenticeshipCompletedEventHandler,
        ILogger<ApprenticeshipCompletedFunction> logger)
    {
        _apprenticeshipCompletedEventHandler = apprenticeshipCompletedEventHandler;
        _logger = logger;
    }

    [FunctionName("ApprenticeshipCompleted")]
    public async Task Run(
        [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.ApprenticeshipCompletedEvent")] ApprenticeshipCompletedEvent message)
    {
        _logger.LogInformation($"Apprenticeship Completed function Begin at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");

        await _apprenticeshipCompletedEventHandler.Handle(message);

        _logger.LogInformation($"Apprenticeship Completed function Finished at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");
    }
}