using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;

namespace SFA.DAS.Forecasting.Commitments.Functions.Functions;

public sealed class ApprenticeshipCompletedFunction(
    IApprenticeshipCompletedEventHandler apprenticeshipCompletedEventHandler,
    ILogger<ApprenticeshipCompletedFunction> logger)
    : IHandleMessages<ApprenticeshipCompletedEvent>
{
    public async Task Handle(ApprenticeshipCompletedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Apprenticeship Completed function started event with ApprenticeshipId: [{ApprenticeshipId}].", message.ApprenticeshipId);

        await apprenticeshipCompletedEventHandler.Handle(message);

        logger.LogInformation("Apprenticeship Completed function completed event with ApprenticeshipId: [{ApprenticeshipId}].", message.ApprenticeshipId);
    }
}