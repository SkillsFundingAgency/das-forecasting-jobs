using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions;

public sealed class ApprenticeshipCompletionDateUpdatedFunction(
    IApprenticeshipCompletionDateUpdatedEventHandler apprenticeshipCompletionDateUpdatedEventHandler,
    ILogger<ApprenticeshipCompletionDateUpdatedFunction> logger)
    : IHandleMessages<ApprenticeshipCompletionDateUpdatedEvent>
{
    public async Task Handle(ApprenticeshipCompletionDateUpdatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation($"Apprenticeship Completion dated update function Begin at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");

        await apprenticeshipCompletionDateUpdatedEventHandler.Handle(message);

        logger.LogInformation($"Apprenticeship Completion date updated function Finished at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");
    }
}