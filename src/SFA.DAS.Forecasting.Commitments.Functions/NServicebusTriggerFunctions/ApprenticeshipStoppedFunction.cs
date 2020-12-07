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
    public class ApprenticeshipStoppedFunction
    {
        private readonly IForecastingDbContext _forecastingDbContext;
        private readonly ICommitmentsApiClient _commitmentsApiClient;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ApprenticeshipStoppedFunction(IForecastingDbContext forecastingDbContext, 
            ICommitmentsApiClient commitmentsApiClient,
            IMapper mapper,
            ILogger<ApprenticeshipStoppedFunction> logger)
        {
            _forecastingDbContext = forecastingDbContext;
            _commitmentsApiClient = commitmentsApiClient;
            _mapper = mapper;
            _logger = logger;
        }

        [FunctionName("ApprenticeshipStopped")]
        public async Task Run(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.ApprenticeshipStopped")] ApprenticeshipStoppedEvent message)
        {
            try
            {
                _logger.LogInformation($"Apprenticeship Stopped function executing at: [{DateTime.UtcNow}] UTC, event with ApprenticeshipId: [{message.ApprenticeshipId}].");

                var selectedApprenticeship = _forecastingDbContext.Commitment.FirstOrDefault(x => x.ApprenticeshipId == message.ApprenticeshipId);
                if (selectedApprenticeship != null)
                {
                    selectedApprenticeship.ActualEndDate = message.StopDate;
                    selectedApprenticeship.Status = Status.Stopped;
                }
                else
                {
                    var apprenticeshipResponse = await _commitmentsApiClient.GetApprenticeship(message.ApprenticeshipId);
                    var result = _mapper.Map<Commitments>(apprenticeshipResponse);
                    result.Status = Status.Stopped;
                    result.ActualEndDate = message.StopDate;
                    _forecastingDbContext.Commitment.Add(result);
                }

                await _forecastingDbContext.SaveChangesAsync();
            }
            catch (CommitmentsApiModelException commitmentException)
            {
                _logger.LogError(commitmentException, $"Apprenticeship Stopped function Failure to retrieve  ApprenticeshipId: [{message.ApprenticeshipId}]");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Apprenticeship Stopped function Failed for ApprenticeshipId: [{message.ApprenticeshipId}] ");
            }
        }
    }
}
