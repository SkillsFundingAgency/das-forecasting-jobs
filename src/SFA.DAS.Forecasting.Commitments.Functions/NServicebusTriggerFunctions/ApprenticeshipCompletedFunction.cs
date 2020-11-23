using Microsoft.Azure.WebJobs;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions
{
    public class ApprenticeshipCompletedFunction
    {
        private readonly IForecastingDbContext _forecastingDbContext;

        public ApprenticeshipCompletedFunction(IForecastingDbContext forecastingDbContext)
        {
            _forecastingDbContext = forecastingDbContext;
        }

        [FunctionName("ApprenticeshipCompleted")]
        public async Task Run(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.ApprenticeshipCompletedEvent")] ApprenticeshipCompletedEvent message)
        {
            var selectedApprenticeship = _forecastingDbContext.Commitment.Where(x => x.ApprenticeshipId == message.ApprenticeshipId).First();
            selectedApprenticeship.ActualEndDate = message.CompletionDate;
            selectedApprenticeship.Status = Status.Completed;
            await _forecastingDbContext.SaveChangesAsync();
        }
    }
}
