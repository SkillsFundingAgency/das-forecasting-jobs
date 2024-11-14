using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;

namespace SFA.DAS.Forecasting.Commitments.Functions.Functions;

public sealed class ApprenticeshipStoppedFunction(
    IApprenticeshipStoppedEventHandler apprenticeshipStoppedEventHandler,
    ILogger<ApprenticeshipStoppedFunction> logger)
    : IHandleMessages<ApprenticeshipStoppedEvent>
{
    private readonly ILogger _logger = logger;

    public async Task Handle(ApprenticeshipStoppedEvent message, IMessageHandlerContext context)
    {
        _logger.LogInformation("Apprenticeship Stopped function started event with ApprenticeshipId: [{ApprenticeshipId}].", message.ApprenticeshipId);

        await apprenticeshipStoppedEventHandler.Handle(message);

        _logger.LogInformation("Apprenticeship Stopped function completed event with ApprenticeshipId: [{ApprenticeshipId}].", message.ApprenticeshipId);
    }
}