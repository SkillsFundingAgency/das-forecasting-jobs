using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.Forecasting.Domain.Triggers
{
    public interface IAccountFundsExpiredTriggerHandler
    {
        Task Handle(AccountFundsExpiredEvent accountFundsExpiredEvent);
    }
}
