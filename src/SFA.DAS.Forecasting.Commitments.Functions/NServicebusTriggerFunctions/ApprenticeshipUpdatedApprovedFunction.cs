using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions
{
    public class ApprenticeshipUpdatedApprovedFunction
    {
        private readonly IApprenticeshipUpdatedApprovedEventHandler _apprenticeshipCompletionDateUpdatedEventHandler;
        private readonly ILogger<ApprenticeshipUpdatedApprovedFunction> _logger;

        public ApprenticeshipUpdatedApprovedFunction(
            IApprenticeshipUpdatedApprovedEventHandler apprenticeshipCompletionDateUpdatedEventHandler,
            ILogger<ApprenticeshipUpdatedApprovedFunction> logger)
        {
            _apprenticeshipCompletionDateUpdatedEventHandler = apprenticeshipCompletionDateUpdatedEventHandler;
            _logger = logger;
        }

        [FunctionName("ApprenticeshipUpdatedApproved")]
        public async Task Run(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.ApprenticeshipUpdatedApproved")] ApprenticeshipUpdatedApprovedEvent message)
        {
            _logger.LogInformation($"Apprenticeship update approved function Begin at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");

            await _apprenticeshipCompletionDateUpdatedEventHandler.Handle(message);

            _logger.LogInformation($"Apprenticeshipupdate update approved  function Finished at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");
        }
    }
}
