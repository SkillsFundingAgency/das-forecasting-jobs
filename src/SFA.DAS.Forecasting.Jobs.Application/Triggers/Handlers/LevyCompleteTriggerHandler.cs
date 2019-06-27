using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Encoding;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Infrastructure;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Models;
using SFA.DAS.Forecasting.Jobs.Infrastructure.Wrappers;

namespace SFA.DAS.Forecasting.Jobs.Application.Triggers.Handlers
{
    public class LevyCompleteTriggerHandler : ILevyCompleteTriggerHandler
    {
        private readonly IOptions<ForecastingJobsConfiguration> _configuration;
        private readonly IHttpFunctionClient<AccountLevyCompleteTrigger> _httpClient;
        private readonly ILogger<LevyCompleteTriggerHandler> _logger;
        private readonly IEncodingService _encodingService;
        private readonly IDateTimeService _dateTimeService;

        public LevyCompleteTriggerHandler(
            IOptions<ForecastingJobsConfiguration> configuration,
            IHttpFunctionClient<AccountLevyCompleteTrigger> httpClient,
            IEncodingService encodingService,
            IDateTimeService dateTimeService,
            ILogger<LevyCompleteTriggerHandler> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
            _encodingService = encodingService;
            _dateTimeService = dateTimeService;
            _httpClient.XFunctionsKey = _configuration.Value.LevyDeclarationPreLoadHttpFunctionXFunctionKey;
        }

        public async Task Handle(RefreshEmployerLevyDataCompletedEvent refreshEmployerLevyDataCompletedEvent)
        {
            try
            {
                var periodMonth = refreshEmployerLevyDataCompletedEvent.PeriodMonth != 0 ? refreshEmployerLevyDataCompletedEvent.PeriodMonth : GetTodayPeriodMonth();
                var periodYear = !string.IsNullOrEmpty(refreshEmployerLevyDataCompletedEvent.PeriodYear) ? refreshEmployerLevyDataCompletedEvent.PeriodYear : GetTodayPeriodYear();  
                var triggerMessage = new AccountLevyCompleteTrigger
                {
                    EmployerAccountIds = new List<string> { _encodingService.Encode(refreshEmployerLevyDataCompletedEvent.AccountId, EncodingType.AccountId) },
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth
                };

                var response = await _httpClient.PostAsync(_configuration.Value.LevyDeclarationPreLoadHttpFunctionBaseUrl, triggerMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to trigger Levy PreLoad HttpTriggerFunction for AccountId: { refreshEmployerLevyDataCompletedEvent.AccountId}, PeriodMonth: { refreshEmployerLevyDataCompletedEvent.PeriodMonth}, PeriodYear: { refreshEmployerLevyDataCompletedEvent.PeriodYear}. Status Code: {response.StatusCode}");
                }

                _logger.LogInformation($"Successfully triggered Levy HttpTriggerFunction for AccountId: { refreshEmployerLevyDataCompletedEvent.AccountId}, PeriodMonth: { refreshEmployerLevyDataCompletedEvent.PeriodMonth}, PeriodYear: { refreshEmployerLevyDataCompletedEvent.PeriodYear}. Status Code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to trigger Levy PreLoad HttpTriggerFunction for AccountId: {refreshEmployerLevyDataCompletedEvent.AccountId}, PeriodMonth: {refreshEmployerLevyDataCompletedEvent.PeriodMonth}, PeriodYear: {refreshEmployerLevyDataCompletedEvent.PeriodYear}");
                throw;
            }
        }

        private string GetTodayPeriodYear()
        {
            var twoDigitYear = int.Parse(_dateTimeService.UtcNow.Year.ToString().Substring(2, 2));
            return _dateTimeService.UtcNow.Month < 4 ? $"{twoDigitYear - 1}-{twoDigitYear}" : $"{twoDigitYear}-{twoDigitYear + 1}";
        }

        private short GetTodayPeriodMonth()
        {
            var month = _dateTimeService.UtcNow.Month;
            return (short) (month >= 4 ? month - 3 : month + 9);
        }
    }
}
