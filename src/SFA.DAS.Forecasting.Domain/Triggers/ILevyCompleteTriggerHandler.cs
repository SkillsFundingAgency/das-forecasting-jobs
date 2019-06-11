using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.Forecasting.Domain.Triggers
{
    public interface ILevyCompleteTriggerHandler
    {
        Task Handle(RefreshEmployerLevyDataCompletedEvent accountLegalEntityAddedEvent);
    }
}
