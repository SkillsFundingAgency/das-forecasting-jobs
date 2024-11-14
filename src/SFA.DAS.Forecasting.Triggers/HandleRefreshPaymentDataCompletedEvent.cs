using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;

namespace SFA.DAS.Forecasting.Triggers;

public sealed class HandleRefreshPaymentDataCompletedEvent (ILogger log,IRefreshPaymentDataCompletedTriggerHandler handler ) : IHandleMessages<RefreshPaymentDataCompletedEvent>
{
    public async Task Handle(RefreshPaymentDataCompletedEvent message, IMessageHandlerContext context)
    {
        log.LogInformation($"NServiceBus {nameof(RefreshPaymentDataCompletedEvent)} trigger function executed at: {DateTime.Now}");
        
        await handler.Handle(message);
    }
}