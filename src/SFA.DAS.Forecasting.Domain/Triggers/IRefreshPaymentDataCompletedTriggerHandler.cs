using SFA.DAS.EmployerFinance.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Domain.Triggers;

public interface IRefreshPaymentDataCompletedTriggerHandler
{
    Task Handle(RefreshPaymentDataCompletedEvent refreshPaymentDataCompletedEvent);
}