using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions
{
    public class ApprenticeshipCompletionDateUpdatedFunction
    {       
        private readonly IApprenticeshipCompletionDateUpdatedEventHandler _apprenticeshipCompletionDateUpdatedEventHandler;
        private readonly ILogger<ApprenticeshipCompletionDateUpdatedFunction> _logger;

        public ApprenticeshipCompletionDateUpdatedFunction(
            IApprenticeshipCompletionDateUpdatedEventHandler apprenticeshipCompletionDateUpdatedEventHandler,
            ILogger<ApprenticeshipCompletionDateUpdatedFunction> logger)
        {          
            _apprenticeshipCompletionDateUpdatedEventHandler = apprenticeshipCompletionDateUpdatedEventHandler;
            _logger = logger;
        }

        [FunctionName("ApprenticeshipCompletionDateUpdated")]
        public async Task Run(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.ApprenticeshipCompletionDateUpdated")] ApprenticeshipCompletionDateUpdatedEvent message)             
        {
            _logger.LogInformation($"Apprenticeship Completion dated update function Begin at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");

            await _apprenticeshipCompletionDateUpdatedEventHandler.Handle(message);

            _logger.LogInformation($"Apprenticeship Completion date updated function Finished at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");
        }
    }
}
