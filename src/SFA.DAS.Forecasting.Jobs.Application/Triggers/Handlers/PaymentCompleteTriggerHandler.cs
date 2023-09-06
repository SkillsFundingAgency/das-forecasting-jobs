using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Services;
using SFA.DAS.Forecasting.Domain.Triggers;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Jobs.Application.Triggers.Handlers;

public class PaymentCompleteTriggerHandler : IRefreshPaymentDataCompletedTriggerHandler
{
    private readonly IPaymentForecastService _paymentForecastService;

    public PaymentCompleteTriggerHandler(IPaymentForecastService paymentForecastService)
    {
        _paymentForecastService = paymentForecastService;
    }

    public async Task Handle(RefreshPaymentDataCompletedEvent refreshPaymentDataCompletedEvent)
    {
        var periodEndDates = GetPeriodDateFromPeriodId(refreshPaymentDataCompletedEvent.PeriodEnd);

        await _paymentForecastService.Trigger(
            periodEndDates.PeriodMonth,
            periodEndDates.PeriodYear,
            refreshPaymentDataCompletedEvent.PeriodEnd,
            refreshPaymentDataCompletedEvent.AccountId);
    }

    private static (short PeriodMonth, int PeriodYear) GetPeriodDateFromPeriodId(string periodId)
    {
        var periodYear = int.Parse("20" + periodId.Substring(0, 2));
        var periodIdMonthAsInt = int.Parse(periodId.Substring(6, 2));
        int periodMonth;

        if (periodIdMonthAsInt > 5)
        {
            periodYear += 1;
            periodMonth = periodIdMonthAsInt - 5;
        }
        else
        {
            periodMonth = periodIdMonthAsInt + 7;
        }

        return ((short)periodMonth, periodYear);
    }
}