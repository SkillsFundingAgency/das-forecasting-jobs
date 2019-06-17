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

namespace SFA.DAS.Forecasting.Jobs.Application.Triggers.Handlers
{
    public class PaymentCompleteTriggerHandler : IRefreshPaymentDataCompletedTriggerHandler
    {
        private readonly IOptions<ForecastingJobsConfiguration> _configuration;
        private readonly IHttpFunctionClient<PaymentDataCompleteTrigger> _httpClient;
        private readonly ILogger _logger;
        private readonly IEncodingService _encodingService;

        public PaymentCompleteTriggerHandler(
            IOptions<ForecastingJobsConfiguration> configuration,
            IHttpFunctionClient<PaymentDataCompleteTrigger> httpClient,
            IEncodingService encodingService,
            ILogger logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
            _encodingService = encodingService;
            _httpClient.XFunctionsKey = _configuration.Value.PaymentPreLoadHttpFunctionXFunctionKey;
        }

        public async Task Handle(RefreshPaymentDataCompletedEvent refreshPaymentDataCompletedEvent)
        {
            try
            {
                var periodDate = GetPeriodDateFromPeriodId(refreshPaymentDataCompletedEvent.PeriodEnd);
                var triggerMessage = new PaymentDataCompleteTrigger
                {
                    EmployerAccountIds = new List<string> { _encodingService.Encode(refreshPaymentDataCompletedEvent.AccountId, EncodingType.PublicAccountId) },
                    PeriodYear = periodDate.PeriodYear,
                    PeriodMonth = periodDate.PeriodMonth,
                    PeriodId = refreshPaymentDataCompletedEvent.PeriodEnd
                };

                var response = await _httpClient.PostAsync(_configuration.Value.PaymentPreLoadHttpFunctionBaseUrl, triggerMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to trigger Payment PreLoad HttpTriggerFunction for AccountId: { refreshPaymentDataCompletedEvent.AccountId}, PeriodEnd: { refreshPaymentDataCompletedEvent.PeriodEnd}. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to trigger Payment PreLoad HttpTriggerFunction for AccountId: {refreshPaymentDataCompletedEvent.AccountId}, PeriodEnd: {refreshPaymentDataCompletedEvent.PeriodEnd}");
                throw;
            }
        }

        private static (int PeriodMonth, int PeriodYear) GetPeriodDateFromPeriodId(string periodId)
        {
            var periodYear = int.Parse("20"+periodId.Substring(0, 2));
            var periodIdMonthAsInt = int.Parse(periodId.Substring(6, 2));
            var periodMonth = periodIdMonthAsInt > 5 ? periodIdMonthAsInt - 5 : periodIdMonthAsInt + 7;

            return (periodMonth, periodYear);
        }
    }
}
