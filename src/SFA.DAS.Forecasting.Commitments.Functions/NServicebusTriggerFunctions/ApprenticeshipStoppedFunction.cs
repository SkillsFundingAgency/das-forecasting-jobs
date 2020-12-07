using AutoMapper;
using Microsoft.Azure.WebJobs;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions
{
    public class ApprenticeshipStoppedFunction
    {
        private readonly IForecastingDbContext _forecastingDbContext;
        private readonly ICommitmentsApiClient _commitmentsApiClient;
        private readonly IMapper _mapper;

        public ApprenticeshipStoppedFunction(IForecastingDbContext forecastingDbContext, 
            ICommitmentsApiClient commitmentsApiClient,
            IMapper mapper)
        {
            _forecastingDbContext = forecastingDbContext;
            _commitmentsApiClient = commitmentsApiClient;
            _mapper = mapper;
        }

        [FunctionName("ApprenticeshipStopped")]
        public async Task Run(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.ApprenticeshipStopped")] ApprenticeshipStoppedEvent message)
        {
            var selectedApprenticeship = _forecastingDbContext.Commitment.FirstOrDefault(x => x.ApprenticeshipId == message.ApprenticeshipId);
            if (selectedApprenticeship != null)
            {
                selectedApprenticeship.ActualEndDate = message.StopDate;
                selectedApprenticeship.Status = Status.Stopped;
            }
            else
            {
                var apprenticeshipResponse = await _commitmentsApiClient.GetApprenticeship(message.ApprenticeshipId); //147108                    
                var result = _mapper.Map<Commitments>(apprenticeshipResponse);
                result.Status = Status.Stopped;
                result.ActualEndDate = message.StopDate;
                _forecastingDbContext.Commitment.Add(result);
            }

            await _forecastingDbContext.SaveChangesAsync();
        }
    }
}
