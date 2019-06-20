using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Jobs.Infrastructure.Attributes;
using SFA.DAS.Forecasting.Jobs.Infrastructure.NServicebus;

namespace SFA.DAS.Forecasting.Triggers
{
    public static class HandleRefreshEmployerLevyDataCompleted
    {
        [FunctionName("HandleRefreshEmployerLevyDataCompleted")]
        public static async Task Run(
            [NServiceBusTrigger(EndPoint = "SFA.DAS.Fcast.Jobs.EmployerLevyDataRefreshed")]RefreshEmployerLevyDataCompletedEvent message,
            [Inject] ILevyCompleteTriggerHandler handler,
            [Inject]ILogger<RefreshEmployerLevyDataCompletedEvent> log)
        {
            log.LogInformation($"NServiceBus {nameof(RefreshEmployerLevyDataCompletedEvent)} trigger function executed at: {DateTime.Now}");
            await handler.Handle(message);
        }
    }
}
