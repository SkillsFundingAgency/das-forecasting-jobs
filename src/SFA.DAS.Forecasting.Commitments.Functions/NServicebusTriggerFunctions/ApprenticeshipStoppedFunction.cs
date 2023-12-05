using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions;

public class ApprenticeshipStoppedFunction
{
    private readonly IApprenticeshipStoppedEventHandler _apprenticeshipStoppedEventHandler;
    private readonly ILogger _logger;

    public ApprenticeshipStoppedFunction(
        IApprenticeshipStoppedEventHandler apprenticeshipStoppedEventHandler,
        ILogger<ApprenticeshipStoppedFunction> logger)
    {
        _apprenticeshipStoppedEventHandler = apprenticeshipStoppedEventHandler;
        _logger = logger;
    }

    [FunctionName(FunctionNames.ApprenticeshipStopped)]
    public async Task Run(
        [NServiceBusTrigger(Endpoint = EndpointNames.ApprenticeshipStopped)]
        ApprenticeshipStoppedEvent message)
    {
        _logger.LogInformation(
            $"Apprenticeship Stopped function Begin at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");

        await _apprenticeshipStoppedEventHandler.Handle(message);

        _logger.LogInformation(
            $"Apprenticeship Stopped function Finished at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");
    }
}