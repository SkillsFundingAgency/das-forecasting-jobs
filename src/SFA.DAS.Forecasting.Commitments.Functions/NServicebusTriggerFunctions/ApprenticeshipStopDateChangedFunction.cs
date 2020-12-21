using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions
{
    public class ApprenticeshipStopDateChangedFunction
    {
        private readonly IApprenticeshipStopDateChangedEventHandler _apprenticeshipStopDateChangedEventHandler;
        private readonly ILogger<ApprenticeshipStopDateChangedFunction> _logger;

        public ApprenticeshipStopDateChangedFunction(
            IApprenticeshipStopDateChangedEventHandler apprenticeshipCompletionDateUpdatedEventHandler,
            ILogger<ApprenticeshipStopDateChangedFunction> logger)
        {
            _apprenticeshipStopDateChangedEventHandler = apprenticeshipCompletionDateUpdatedEventHandler;
            _logger = logger;
        }

        [FunctionName("ApprenticeshipStopDateChanged")]
        public async Task Run(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.ApprenticeshipStopDateChanged")] ApprenticeshipStopDateChangedEvent message)
        {
            _logger.LogInformation($"Apprenticeship update approved function Begin at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");

            await _apprenticeshipStopDateChangedEventHandler.Handle(message);

            _logger.LogInformation($"Apprenticeshipupdate update approved  function Finished at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");
        }
    }
}
