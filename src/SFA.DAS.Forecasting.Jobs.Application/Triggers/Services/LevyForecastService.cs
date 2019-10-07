using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Infrastructure;
using SFA.DAS.Forecasting.Domain.Services;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Models;

namespace SFA.DAS.Forecasting.Jobs.Application.Triggers.Services
{
    public class LevyForecastService : ILevyForecastService
    {
        private readonly IOptions<ForecastingJobsConfiguration> _configuration;
        private readonly IHttpFunctionClient<AccountLevyCompleteTrigger> _httpClient;
        private readonly ILogger<LevyForecastService> _logger;
        private readonly IEncodingService _encodingService;

        public LevyForecastService(
            IOptions<ForecastingJobsConfiguration> configuration,
            IHttpFunctionClient<AccountLevyCompleteTrigger> httpClient,
            IEncodingService encodingService,
            ILogger<LevyForecastService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
            _encodingService = encodingService;
            _httpClient.XFunctionsKey = _configuration.Value.LevyDeclarationPreLoadHttpFunctionXFunctionKey;
        }

        public async Task TriggerLevyForecast(short periodMonth, string periodYear, long accountId)
        {
            try
            {
                var triggerMessage = new AccountLevyCompleteTrigger
                {
                    EmployerAccountIds = new List<string> { _encodingService.Encode(accountId, EncodingType.AccountId) },
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth
                };

                var response = await _httpClient.PostAsync(_configuration.Value.LevyDeclarationPreLoadHttpFunctionBaseUrl, triggerMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to trigger Levy PreLoad HttpTriggerFunction for AccountId: { accountId }, PeriodMonth: { periodMonth }, PeriodYear: { periodYear }. Status Code: {response.StatusCode}");
                }

                _logger.LogInformation($"Successfully triggered Levy HttpTriggerFunction for AccountId: { accountId }, PeriodMonth: { periodMonth }, PeriodYear: { periodYear }. Status Code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to trigger Levy PreLoad HttpTriggerFunction for AccountId: {accountId }, PeriodMonth: { periodMonth }, PeriodYear: { periodYear }");
                throw;
            }
        }
    }
}
