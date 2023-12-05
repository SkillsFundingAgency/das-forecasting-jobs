using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Jobs.Infrastructure.Attributes;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;

namespace SFA.DAS.Forecasting.Triggers;

public static class HandleRefreshEmployerLevyDataCompleted
{
    [FunctionName("HandleRefreshEmployerLevyDataCompleted")]
    public static void Run(
        [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.Jobs.EmployerLevyDataRefreshed")] RefreshEmployerLevyDataCompletedEvent message,
        [Inject] ILevyCompleteTriggerHandler handler,
        [Inject] ILogger<RefreshEmployerLevyDataCompletedEvent> log)
    {
        log.LogInformation($"NServiceBus {nameof(RefreshEmployerLevyDataCompletedEvent)} trigger function executed at: {DateTime.Now}");
        handler.Handle(message);
    }
}