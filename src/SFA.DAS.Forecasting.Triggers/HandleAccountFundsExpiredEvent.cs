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
    public static class HandleAccountFundsExpiredEvent
    {
        [FunctionName("HandleAccountFundsExpiredEvent")]
        public static async Task Run(
            [NServiceBusTrigger(EndPoint = "SFA.DAS.Forecasting.Jobs.AccountFundsExpiredEvent")]AccountFundsExpiredEvent message, 
            [Inject] ILevyCompleteTriggerHandler handler,
            ILogger log)
        {
            log.LogInformation($"NServiceBus {nameof(AccountFundsExpiredEvent)} trigger function executed at: {DateTime.Now}");
            var convertedMessage = new RefreshEmployerLevyDataCompletedEvent
            {
                AccountId = message.AccountId,
                PeriodYear = ConvertDateToPeriodYear(message.Created),
                PeriodMonth = (short)message.Created.Month
            };
            await handler.Handle(convertedMessage);
        }

        private static string ConvertDateToPeriodYear(DateTime date)
        {
            return date.Year.ToString().Substring(2, 2) + "-" +
                   (date.Year + 1).ToString().Substring(2, 2);
        }
    }
}
