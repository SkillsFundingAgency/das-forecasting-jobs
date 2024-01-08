using System.Collections.Generic;

namespace SFA.DAS.Forecasting.Jobs.Application.Triggers.Models;

public class PaymentDataCompleteTrigger
{
    public List<long> EmployerAccountIds { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public string PeriodId { get; set; }
}