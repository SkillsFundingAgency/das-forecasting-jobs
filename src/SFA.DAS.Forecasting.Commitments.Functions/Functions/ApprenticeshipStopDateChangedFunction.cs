using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;

namespace SFA.DAS.Forecasting.Commitments.Functions.Functions;

public class ApprenticeshipStopDateChangedFunction(
    IApprenticeshipStopDateChangedEventHandler apprenticeshipCompletionDateUpdatedEventHandler,
    ILogger<ApprenticeshipStopDateChangedFunction> logger)
    : IHandleMessages<ApprenticeshipStopDateChangedEvent>
{
    public async Task Handle(ApprenticeshipStopDateChangedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Apprenticeship update approved function started event with ApprenticeshipId: [{ApprenticeshipId}].", message.ApprenticeshipId);

        await apprenticeshipCompletionDateUpdatedEventHandler.Handle(message);

        logger.LogInformation("ApprenticeshipUpdate update approved  function completed event with ApprenticeshipId: [{ApprenticeshipId}].", message.ApprenticeshipId);
    }
}