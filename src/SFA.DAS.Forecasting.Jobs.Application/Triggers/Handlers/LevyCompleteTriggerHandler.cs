using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Infrastructure;
using SFA.DAS.Forecasting.Domain.Triggers;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Models;

namespace SFA.DAS.Forecasting.Jobs.Application.Triggers.Handlers
{
    public class LevyCompleteTriggerHandler : ILevyCompleteTriggerHandler
    {
        private readonly IOptions<ForecastingJobsConfiguration> _configuration;
        private readonly IHttpFunctionClient<AccountLevyCompleteTrigger> _httpClient;
        private readonly ILogger _logger;

        public LevyCompleteTriggerHandler(
            IOptions<ForecastingJobsConfiguration> configuration,
            IHttpFunctionClient<AccountLevyCompleteTrigger> httpClient,
            ILogger logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.XFunctionsKey = _configuration.Value.LevyDeclarationPreLoadHttpFunctionXFunctionKey;
        }

        public async Task Handle(RefreshEmployerLevyDataCompletedEvent refreshEmployerLevyDataCompletedEvent)
        {
            try
            {
                var triggerMessage = new AccountLevyCompleteTrigger
                {
                    EmployerAccountIds = new List<string> { refreshEmployerLevyDataCompletedEvent.AccountId.ToString() },
                    PeriodYear = refreshEmployerLevyDataCompletedEvent.PeriodYear,
                    PeriodMonth = refreshEmployerLevyDataCompletedEvent.PeriodMonth
                };

                var response = await _httpClient.PostAsync(_configuration.Value.LevyDeclarationPreLoadHttpFunctionBaseUrl, triggerMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to trigger Levy PreLoad HttpTriggerFunction for AccountId: { refreshEmployerLevyDataCompletedEvent.AccountId}, PeriodMonth: { refreshEmployerLevyDataCompletedEvent.PeriodMonth}, PeriodYear: { refreshEmployerLevyDataCompletedEvent.PeriodYear}. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to trigger Levy PreLoad HttpTriggerFunction for AccountId: {refreshEmployerLevyDataCompletedEvent.AccountId}, PeriodMonth: {refreshEmployerLevyDataCompletedEvent.PeriodMonth}, PeriodYear: {refreshEmployerLevyDataCompletedEvent.PeriodYear}");
                throw;
            }
        }
    }
}
