using SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Models;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Services;

public interface IGetApprenticeshipService
{
    Task<Commitments> GetApprenticeshipDetails(long apprenticeshipId);
}