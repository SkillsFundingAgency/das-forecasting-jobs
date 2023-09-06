using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Domain.Services;

public interface ILevyForecastService
{
    Task Trigger(short periodMonth, string periodYear, long accountId);
}