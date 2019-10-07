using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Services;
using SFA.DAS.Forecasting.Domain.Triggers;

namespace SFA.DAS.Forecasting.Jobs.Application.Triggers.Handlers
{
    public class LevyCompleteTriggerHandler : ILevyCompleteTriggerHandler
    {
        private readonly ILevyForecastService _triggerLevyForecastService;

        public LevyCompleteTriggerHandler(
            ILevyForecastService triggerLevyForecastService)
        {
            _triggerLevyForecastService = triggerLevyForecastService;
        }

        public async Task Handle(RefreshEmployerLevyDataCompletedEvent refreshEmployerLevyDataCompletedEvent)
        {
            if (!refreshEmployerLevyDataCompletedEvent.LevyImported)
            {
                return;
            }

            var periodMonth = refreshEmployerLevyDataCompletedEvent.PeriodMonth != 0 
                ? refreshEmployerLevyDataCompletedEvent.PeriodMonth 
                : GetTodayPeriodMonth(refreshEmployerLevyDataCompletedEvent.Created);
            var periodYear = !string.IsNullOrEmpty(refreshEmployerLevyDataCompletedEvent.PeriodYear) 
                ? refreshEmployerLevyDataCompletedEvent.PeriodYear 
                : GetTodayPeriodYear(refreshEmployerLevyDataCompletedEvent.Created);

            await _triggerLevyForecastService.Trigger(periodMonth, periodYear, refreshEmployerLevyDataCompletedEvent.AccountId);
        }

        private string GetTodayPeriodYear(DateTime eventCreatedDate)
        {
            var twoDigitYear = int.Parse(eventCreatedDate.ToString("yy"));
            return eventCreatedDate.Month < 4 ? $"{twoDigitYear - 1}-{twoDigitYear}" : $"{twoDigitYear}-{twoDigitYear + 1}";
        }

        private short GetTodayPeriodMonth(DateTime eventCreatedDate)
        {
            var month = eventCreatedDate.Month;
            return (short)(month >= 4 ? month - 3 : month + 9);
        }
    }
}
