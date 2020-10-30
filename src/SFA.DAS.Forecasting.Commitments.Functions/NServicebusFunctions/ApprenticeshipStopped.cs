using Microsoft.Azure.WebJobs;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusFunctions
{
    public class ApprenticeshipStopped
    {
        private readonly IForecastingDbContext _forecastingDbContext;

        public ApprenticeshipStopped(IForecastingDbContext forecastingDbContext)
        {
            _forecastingDbContext = forecastingDbContext;
        }

        [FunctionName("ApprenticeshipStopped")]
        public async Task Run(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.ApprenticeshipStopped")] ApprenticeshipStoppedEvent message)
        {
           // message.ApprenticeshipId = 2;
            var selectedApprenticeship = _forecastingDbContext.Commitment.Where(x => x.ApprenticeshipId == message.ApprenticeshipId).First();
            selectedApprenticeship.ActualEndDate = message.StopDate;
            selectedApprenticeship.Status = Status.Stopped;
            _forecastingDbContext.SaveChanges();
            await Task.FromResult(0);
        }
    }
}
