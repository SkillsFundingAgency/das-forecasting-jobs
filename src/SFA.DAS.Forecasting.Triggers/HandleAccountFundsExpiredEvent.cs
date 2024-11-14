using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;

namespace SFA.DAS.Forecasting.Triggers;

public sealed class HandleAccountFundsExpiredEvent(ILogger log, ILevyCompleteTriggerHandler handler): IHandleMessages<AccountFundsExpiredEvent>
{
    public async Task Handle(AccountFundsExpiredEvent @event, IMessageHandlerContext context)
    {
        log.LogInformation($"NServiceBus {nameof(AccountFundsExpiredEvent)} trigger function executed at: {DateTime.Now}");

        var convertedMessage = new RefreshEmployerLevyDataCompletedEvent
        {
            AccountId = @event.AccountId,
            Created = @event.Created,
            // Allow forecasting to be triggered for Expiry event
            LevyImported = true
        };

        await handler.Handle(convertedMessage);
    }
}