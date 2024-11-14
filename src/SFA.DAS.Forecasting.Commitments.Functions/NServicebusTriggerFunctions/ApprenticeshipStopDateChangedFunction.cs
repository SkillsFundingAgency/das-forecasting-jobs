using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions;

public class ApprenticeshipStopDateChangedFunction(
    IApprenticeshipStopDateChangedEventHandler apprenticeshipCompletionDateUpdatedEventHandler,
    ILogger<ApprenticeshipStopDateChangedFunction> logger)
    : IHandleMessages<ApprenticeshipStopDateChangedEvent>
{
    public async Task Handle(ApprenticeshipStopDateChangedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation($"Apprenticeship update approved function Begin at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");

        await apprenticeshipCompletionDateUpdatedEventHandler.Handle(message);

        logger.LogInformation($"ApprenticeshipUpdate update approved  function Finished at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");
    }
}