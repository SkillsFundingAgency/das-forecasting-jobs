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
    public class ApprenticeshipCompletionDateUpdatedEventHandler : IApprenticeshipCompletionDateUpdatedEventHandler
    {
        private readonly IForecastingDbContext _forecastingDbContext;
        private readonly IMapper _mapper;
        private readonly ICommitmentsApiClient _commitmentsApiClient;
        private readonly ILogger _logger;

        public ApprenticeshipCompletionDateUpdatedEventHandler(IForecastingDbContext forecastingDbContext,
            ICommitmentsApiClient commitmentsApiClient,
            IMapper mapper,
            ILogger<ApprenticeshipCompletionDateUpdatedEventHandler> logger)
        {
            _forecastingDbContext = forecastingDbContext;
            _commitmentsApiClient = commitmentsApiClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipCompletionDateUpdatedEvent message)
        {
            try
            {
                var selectedApprenticeship = _forecastingDbContext.Commitment.FirstOrDefault(x => x.ApprenticeshipId == message.ApprenticeshipId);
                if (selectedApprenticeship == null)
                {
                    var apprenticeshipResponse = await _commitmentsApiClient.GetApprenticeship(message.ApprenticeshipId);
                    selectedApprenticeship = _mapper.Map<Commitments>(apprenticeshipResponse);
                    _forecastingDbContext.Commitment.Add(selectedApprenticeship);
                }

                selectedApprenticeship.Status = Status.Completed;
                selectedApprenticeship.ActualEndDate = message.CompletionDate;
                await _forecastingDbContext.SaveChangesAsync();
            }
            catch (CommitmentsApiModelException commitmentException)
            {
                _logger.LogError(commitmentException, $"Apprenticeship Completion Date updated function Failure to retrieve  ApprenticeshipId: [{message.ApprenticeshipId}]");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Apprenticeship Completion Date updated function Failed for ApprenticeshipId: [{message.ApprenticeshipId}] ");
                throw;
            }
        }
    }
}
