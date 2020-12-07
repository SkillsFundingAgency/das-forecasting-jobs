using AutoMapper;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions
{
    public class ApprenticeshipCompletedFunction
    {
        private readonly IForecastingDbContext _forecastingDbContext;
        private readonly IMapper _mapper;
        private readonly ICommitmentsApiClient _commitmentsApiClient;
        private readonly ILogger _logger;

        public ApprenticeshipCompletedFunction(IForecastingDbContext forecastingDbContext, 
            ICommitmentsApiClient commitmentsApiClient,
            IMapper mapper,
            ILogger<ApprenticeshipCompletedFunction> logger)
        {
            _forecastingDbContext = forecastingDbContext;
            _commitmentsApiClient = commitmentsApiClient;
            _mapper = mapper;
            _logger = logger;
        }

        [FunctionName("ApprenticeshipCompleted")]
        public async Task Run(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.ApprenticeshipCompletedEvent")] ApprenticeshipCompletedEvent message)
        {
            try
            {
                _logger.LogInformation($"Apprenticeship Completed function executing at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");               

                var selectedApprenticeship = _forecastingDbContext.Commitment.FirstOrDefault(x => x.ApprenticeshipId == message.ApprenticeshipId);
                if (selectedApprenticeship != null)
                {
                    selectedApprenticeship.ActualEndDate = message.CompletionDate;
                    selectedApprenticeship.Status = Status.Completed;
                }
                else
                {
                    var apprenticeshipResponse = await _commitmentsApiClient.GetApprenticeship(message.ApprenticeshipId); //147108                    
                    var result = _mapper.Map<Commitments>(apprenticeshipResponse);
                    result.Status = Status.Completed;
                    result.ActualEndDate = message.CompletionDate;
                    _forecastingDbContext.Commitment.Add(result);
                }

                await _forecastingDbContext.SaveChangesAsync();
            }
            catch(CommitmentsApiModelException commitmentException )
            {
                _logger.LogError(commitmentException, $"Failure to retrieve  ApprenticeshipId: [{message.ApprenticeshipId}]");
            }
            catch (Exception ex)
            {              
                _logger.LogError(ex, $"Apprenticeship Completed function Failed for ApprenticeshipId: [{message.ApprenticeshipId}] ");                
            }
        }
    }
}
