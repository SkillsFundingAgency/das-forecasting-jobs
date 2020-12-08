using Microsoft.Azure.WebJobs;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions
{
    public class ApprenticeshipCompletedFunction
    {
       // private readonly IForecastingDbContext _forecastingDbContext;

        //public ApprenticeshipCompletedFunction(IForecastingDbContext forecastingDbContext)
        public ApprenticeshipCompletedFunction()
        {
            //_forecastingDbContext = forecastingDbContext;
        }

        [FunctionName("ApprenticeshipCompleted")]
        public async Task Run(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.ApprenticeshipCompletedEvent")] ApprenticeshipCompletedEvent message)
        {
          //  await _forecastingDbContext.SaveChangesAsync(6333998, Status.Completed, message.CompletionDate);
            //var selectedApprenticeship = _forecastingDbContext.Commitment.FirstOrDefault(x => x.ApprenticeshipId == message.ApprenticeshipId);
            //if (selectedApprenticeship != null)
            //{
            //    selectedApprenticeship.ActualEndDate = message.CompletionDate;
            //    selectedApprenticeship.Status = Status.Completed;
            //    await _forecastingDbContext.SaveChangesAsync();
            //}
        }
    }
}
