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
    public class ApprenticeshipUpdatedApprovedEventHandler : IApprenticeshipUpdatedApprovedEventHandler
    {
        private readonly IForecastingDbContext _forecastingDbContext;
        private readonly IMapper _mapper;
        private readonly ICommitmentsApiClient _commitmentsApiClient;
        private readonly ILogger _logger;

        public ApprenticeshipUpdatedApprovedEventHandler(IForecastingDbContext forecastingDbContext,
            ICommitmentsApiClient commitmentsApiClient,
            IMapper mapper,
            ILogger<ApprenticeshipUpdatedApprovedEventHandler> logger)
        {
            _forecastingDbContext = forecastingDbContext;
            _commitmentsApiClient = commitmentsApiClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipUpdatedApprovedEvent message)
        {
            try
            {
                if (message.EndDate != DateTime.MinValue)
                {
                    var selectedApprenticeship = _forecastingDbContext.Commitment.FirstOrDefault(x => x.ApprenticeshipId == message.ApprenticeshipId);
                    if (selectedApprenticeship == null)
                    {
                        var apprenticeshipResponse = await _commitmentsApiClient.GetApprenticeship(message.ApprenticeshipId);
                        selectedApprenticeship = _mapper.Map<Commitments>(apprenticeshipResponse);
                        _forecastingDbContext.Commitment.Add(selectedApprenticeship);
                    }

                    selectedApprenticeship.Status = Status.Stopped;
                    selectedApprenticeship.ActualEndDate = message.EndDate;
                    await _forecastingDbContext.SaveChangesAsync();
                }
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
