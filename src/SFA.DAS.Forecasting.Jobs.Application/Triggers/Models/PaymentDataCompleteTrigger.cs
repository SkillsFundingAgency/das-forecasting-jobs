using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Forecasting.Jobs.Application.Triggers.Models
{
    public class PaymentDataCompleteTrigger
    {
        public List<string> EmployerAccountIds { get; set; }
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public string PeriodId { get; set; }
    }
}
