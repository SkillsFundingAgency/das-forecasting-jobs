using System;

namespace SFA.DAS.Forecasting.Jobs.Infrastructure.Wrappers
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
