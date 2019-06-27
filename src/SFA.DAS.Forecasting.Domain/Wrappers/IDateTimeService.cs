using System;

namespace SFA.DAS.Forecasting.Jobs.Infrastructure.Wrappers
{
    public interface IDateTimeService
    {
        DateTime UtcNow { get; }
    }
}
