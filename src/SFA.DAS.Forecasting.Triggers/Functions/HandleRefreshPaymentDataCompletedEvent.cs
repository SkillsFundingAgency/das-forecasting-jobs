using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;

namespace SFA.DAS.Forecasting.Triggers.Functions;

public sealed class HandleRefreshPaymentDataCompletedEvent(ILogger log, IRefreshPaymentDataCompletedTriggerHandler handler) : IHandleMessages<RefreshPaymentDataCompletedEvent>
{
    public async Task Handle(RefreshPaymentDataCompletedEvent message, IMessageHandlerContext context)
    {
        log.LogInformation("NServiceBus {TypeName} trigger function executed.", nameof(RefreshPaymentDataCompletedEvent));

        await handler.Handle(message);
    }
}