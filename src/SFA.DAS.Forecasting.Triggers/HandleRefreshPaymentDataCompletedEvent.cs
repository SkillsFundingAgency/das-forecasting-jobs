using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Jobs.Infrastructure.Attributes;
using SFA.DAS.NServiceBus.AzureFunction.Infrastructure;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Triggers;

public static class HandleRefreshPaymentDataCompletedEvent
{
    [FunctionName("HandleRefreshPaymentDataCompletedEvent")]
    public static async Task Run(
        [NServiceBusTrigger(EndPoint = "SFA.DAS.Fcast.Jobs.PaymentDataRefreshed")] RefreshPaymentDataCompletedEvent message,
        [Inject] IRefreshPaymentDataCompletedTriggerHandler handler,
        [Inject] ILogger<RefreshPaymentDataCompletedEvent> log)
    {
        log.LogInformation($"NServiceBus {nameof(RefreshPaymentDataCompletedEvent)} trigger function executed at: {DateTime.Now}");
        await handler.Handle(message);
    }
}