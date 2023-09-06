using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Infrastructure;
using SFA.DAS.Forecasting.Domain.Services;
using SFA.DAS.Forecasting.Jobs.Application.Triggers.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Jobs.Application.Triggers.Services;

public class PaymentForecastService : IPaymentForecastService
{
    private ForecastingJobsConfiguration _configuration;
    private IHttpFunctionClient<PaymentDataCompleteTrigger> _httpFunctionClient;
    private ILogger<PaymentForecastService> _logger;

    public PaymentForecastService(
        IOptions<ForecastingJobsConfiguration> options,
        IHttpFunctionClient<PaymentDataCompleteTrigger> httpFunctionClient,
        ILogger<PaymentForecastService> logger)
    {
        _configuration = options.Value;
        _httpFunctionClient = httpFunctionClient;
        _logger = logger;
        _httpFunctionClient.XFunctionsKey = _configuration.PaymentPreLoadHttpFunctionXFunctionKey;
    }

    public async Task Trigger(short periodMonth, int periodYear, string periodEnd, long accountId)
    {
        try
        {
            var triggerMessage = new PaymentDataCompleteTrigger
            {
                EmployerAccountIds = new List<long> { accountId },
                PeriodYear = periodYear,
                PeriodMonth = periodMonth,
                PeriodId = periodEnd
            };

            var response = await _httpFunctionClient.PostAsync(_configuration.PaymentPreLoadHttpFunctionBaseUrl, triggerMessage);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to trigger Payment PreLoad HttpTriggerFunction for AccountId: {accountId}, PeriodEnd: {periodEnd}. Status Code: {response.StatusCode}");
                throw new Exception($"Status Code: {response.StatusCode}, reason: {response.ReasonPhrase}");
            }

            _logger.LogInformation($"Successfully triggered Payment PreLoad HttpTriggerFunction for AccountId: {accountId}, PeriodEnd: {periodEnd}, Status Code: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to trigger Payment PreLoad HttpTriggerFunction for AccountId: {accountId}, PeriodEnd: {periodEnd}");
            throw;
        }
    }
}