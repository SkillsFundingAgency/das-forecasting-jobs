using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;

namespace SFA.DAS.Forecasting.Triggers;

public sealed class HandleRefreshEmployerLevyDataCompleted(ILogger log, ILevyCompleteTriggerHandler handler): IHandleMessages<RefreshEmployerLevyDataCompletedEvent>
{
    public async Task Handle(RefreshEmployerLevyDataCompletedEvent @event, IMessageHandlerContext context)
    {
        log.LogInformation($"NServiceBus {nameof(RefreshEmployerLevyDataCompletedEvent)} trigger function executed at: {DateTime.Now}");
        
        await handler.Handle(@event);
    }
}