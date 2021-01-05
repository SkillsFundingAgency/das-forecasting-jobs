using AutoMapper;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Models;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Services;
using SFA.DAS.Forecasting.Jobs.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Handlers
{
    public class ApprenticeshipStopDateChangedEventHandler : IApprenticeshipStopDateChangedEventHandler
    {
        private readonly IForecastingDbContext _forecastingDbContext;
        private readonly IGetApprenticeshipService _getApprenticeshipService;
        private readonly ILogger _logger;

        public ApprenticeshipStopDateChangedEventHandler(IForecastingDbContext forecastingDbContext,
            IGetApprenticeshipService getApprenticeshipService,
            ILogger<ApprenticeshipStopDateChangedEventHandler> logger)
        {
            _forecastingDbContext = forecastingDbContext;
            _getApprenticeshipService = getApprenticeshipService;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipStopDateChangedEvent message)
        {
            try
            {
                var selectedApprenticeship = _forecastingDbContext.Commitment.FirstOrDefault(x => x.ApprenticeshipId == message.ApprenticeshipId);
                if (selectedApprenticeship == null)
                {
                    selectedApprenticeship = await _getApprenticeshipService.GetApprenticeshipDetails(message.ApprenticeshipId);
                    _forecastingDbContext.Commitment.Add(selectedApprenticeship);
                }

                selectedApprenticeship.UpdatedDateTime = DateTime.UtcNow;
                selectedApprenticeship.Status = Status.Stopped;
                selectedApprenticeship.ActualEndDate = message.StopDate;
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
