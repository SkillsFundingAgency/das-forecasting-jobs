using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Domain.CommitmentsFunctions;

public interface IApprenticeshipStoppedEventHandler
{
    Task Handle(ApprenticeshipStoppedEvent apprenticeshipCompletedEvent);
}