using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Jobs.Infrastructure.Attributes;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;

namespace SFA.DAS.Forecasting.Triggers;

public static class HandleAccountFundsExpiredEvent
{
    [FunctionName(FunctionNames.HandleAccountFundsExpiredEvent)]
    public static async Task Run(
        [NServiceBusTrigger(Endpoint = EndpointNames.FundsExpired)]
        AccountFundsExpiredEvent message,
        [Inject] ILevyCompleteTriggerHandler handler,
        [Inject] ILogger<AccountFundsExpiredEvent> log)
    {
        log.LogInformation($"NServiceBus {nameof(AccountFundsExpiredEvent)} trigger function executed at: {DateTime.Now}");

        var convertedMessage = new RefreshEmployerLevyDataCompletedEvent
        {
            AccountId = message.AccountId,
            Created = message.Created,
            // Allow forecasting to be triggered for Expiry event
            LevyImported = true
        };

        await handler.Handle(convertedMessage);
    }
}