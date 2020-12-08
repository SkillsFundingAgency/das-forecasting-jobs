using Microsoft.Azure.WebJobs;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions
{
    public class ApprenticeshipStoppedFunction
    {
     //   private readonly IForecastingDbContext _forecastingDbContext;

        //public ApprenticeshipStoppedFunction(IForecastingDbContext forecastingDbContext)
             public ApprenticeshipStoppedFunction()
        {
            //_forecastingDbContext = forecastingDbContext;
        }

        [FunctionName("ApprenticeshipStopped")]
        public async Task Run(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.ApprenticeshipStopped")] ApprenticeshipStoppedEvent message)
        {
            //await _forecastingDbContext.SaveChangesAsync(8128443, Status.Stopped, message.StopDate);
            //var selectedApprenticeship = _forecastingDbContext.Commitment.FirstOrDefault(x => x.ApprenticeshipId == message.ApprenticeshipId);
            //if (selectedApprenticeship != null)
            //{
            //    selectedApprenticeship.ActualEndDate = message.StopDate;
            //    selectedApprenticeship.Status = Status.Stopped;
            //    await _forecastingDbContext.SaveChangesAsync();
            //}
        }
    }
}
