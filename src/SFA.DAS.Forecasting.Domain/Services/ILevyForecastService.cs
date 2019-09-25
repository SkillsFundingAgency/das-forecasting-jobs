using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Domain.Services
{
    public interface ILevyForecastService
    {
        Task TriggerLevyForecast(short periodMonth, string periodYear, long accountId);
    }
}
