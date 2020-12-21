using AutoMapper;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using SFA.DAS.Forecasting.Jobs.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Handlers
{
    public class ApprenticeshipCompletedEventHandler : IApprenticeshipCompletedEventHandler
    {
        private readonly IForecastingDbContext _forecastingDbContext;
        private readonly IMapper _mapper;
        private readonly ICommitmentsApiClient _commitmentsApiClient;
        private readonly ILogger _logger;

        public ApprenticeshipCompletedEventHandler(IForecastingDbContext forecastingDbContext,
            ICommitmentsApiClient commitmentsApiClient,
            IMapper mapper,
            ILogger<ApprenticeshipCompletedEventHandler> logger)
        {
            _forecastingDbContext = forecastingDbContext;
            _commitmentsApiClient = commitmentsApiClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipCompletedEvent message)
        {
            try
            {
                var selectedApprenticeship = _forecastingDbContext.Commitment.FirstOrDefault(x => x.ApprenticeshipId == message.ApprenticeshipId);
                if (selectedApprenticeship != null)
                {
                    selectedApprenticeship.ActualEndDate = message.CompletionDate;
                    selectedApprenticeship.Status = Status.Completed;
                }
                else
                {
                    var apprenticeshipResponse = await _commitmentsApiClient.GetApprenticeship(message.ApprenticeshipId);
                    var result = _mapper.Map<Commitments>(apprenticeshipResponse);
                    result.Status = Status.Completed;
                    result.ActualEndDate = message.CompletionDate;
                    _forecastingDbContext.Commitment.Add(result);
                }

                await _forecastingDbContext.SaveChangesAsync();                
            }
            catch (CommitmentsApiModelException commitmentException)
            {
                _logger.LogError(commitmentException, $"Apprenticeship Completed function Failure to retrieve  ApprenticeshipId: [{message.ApprenticeshipId}]");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Apprenticeship Completed function Failed for ApprenticeshipId: [{message.ApprenticeshipId}] ");
                throw;
            }

            
        }
    }
}
