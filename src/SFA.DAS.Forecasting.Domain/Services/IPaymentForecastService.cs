using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Domain.Services
{
    public interface IPaymentForecastService
    {
        Task Trigger(short periodMonth, int periodYear, string periodEnd, long accountId);
    }
}
