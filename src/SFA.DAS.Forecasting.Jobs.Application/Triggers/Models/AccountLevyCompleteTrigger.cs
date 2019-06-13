using System.Collections.Generic;

namespace SFA.DAS.Forecasting.Jobs.Application.Triggers.Models
{
    public class AccountLevyCompleteTrigger
    {
        public List<string> EmployerAccountIds { get; set; }
        public string PeriodYear { get; set; }
        public short PeriodMonth { get; set; }
    }
}
