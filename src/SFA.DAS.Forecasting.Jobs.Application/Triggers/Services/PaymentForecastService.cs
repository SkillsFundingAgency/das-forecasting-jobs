using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Infrastructure;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Models;

namespace SFA.DAS.Forecasting.Jobs.Application.Triggers.Services
{
    public class PaymentForecastService
    {
        private ForecastingJobsConfiguration _configuration;
        private IHttpFunctionClient<PaymentDataCompleteTrigger> _httpFunctionClient;
        private IEncodingService _encodingService;
        private ILogger<PaymentForecastService> _logger;

        public PaymentForecastService(
            IOptions<ForecastingJobsConfiguration> options, 
            IHttpFunctionClient<PaymentDataCompleteTrigger> httpFunctionClient, 
            IEncodingService encodingService, 
            ILogger<PaymentForecastService> logger)
        {
            _configuration = options.Value;
            _httpFunctionClient = httpFunctionClient;
            _encodingService = encodingService;
            _logger = logger;
        }

        public async Task TriggerPaymentForecast(short periodMonth, int periodYear, string periodEnd, long accountId)
        {
            try
            {
                var triggerMessage = new PaymentDataCompleteTrigger
                {
                    EmployerAccountIds = new List<string> { _encodingService.Encode(accountId, EncodingType.AccountId) },
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth,
                    PeriodId = periodEnd
                };

                var response = await _httpFunctionClient.PostAsync(_configuration.PaymentPreLoadHttpFunctionBaseUrl, triggerMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to trigger Payment PreLoad HttpTriggerFunction for AccountId: { accountId}, PeriodEnd: { periodEnd}. Status Code: {response.StatusCode}");
                }

                _logger.LogInformation($"Successfully triggered Payment PreLoad HttpTriggerFunction for AccountId: { accountId}, PeriodEnd: { periodEnd}, Status Code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to trigger Payment PreLoad HttpTriggerFunction for AccountId: {accountId}, PeriodEnd: {periodEnd}");
                throw;
            }
        }
    }
}
