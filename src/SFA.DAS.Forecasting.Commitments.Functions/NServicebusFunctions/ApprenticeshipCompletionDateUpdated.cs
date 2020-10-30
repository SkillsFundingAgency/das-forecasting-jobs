using Microsoft.Azure.WebJobs;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusFunctions
{
    public class ApprenticeshipCompletionDateUpdated
    {
        private readonly IForecastingDbContext _forecastingDbContext;

        public ApprenticeshipCompletionDateUpdated(IForecastingDbContext forecastingDbContext)
        {
            _forecastingDbContext = forecastingDbContext;
        }

        [FunctionName("ApprenticeshipCompletionDateUpdated")]
        public async Task Run(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.CompletionDateUpdated")] ApprenticeshipCompletionDateUpdatedEvent message)
        {
            message.ApprenticeshipId = 2;
            var selectedApprenticeship = _forecastingDbContext.Commitment.Where(x => x.ApprenticeshipId == message.ApprenticeshipId).First();
            selectedApprenticeship.ActualEndDate = message.CompletionDate;
            selectedApprenticeship.Status = Status.Completed;
            _forecastingDbContext.SaveChanges();
            await Task.FromResult(0);
        }
    }
}
