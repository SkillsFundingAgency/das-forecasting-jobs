namespace SFA.DAS.Forecasting.Domain.Configuration
{
    public class ForecastingJobsConfiguration
    {
        public string ServiceBusConnectionString { get; set; }
        public string LevyDeclarationPreLoadHttpFunctionBaseUrl { get; set; }
        public string LevyDeclarationPreLoadHttpFunctionXFunctionKey { get; set; }
        public string PaymentPreLoadHttpFunctionBaseUrl { get; set; }
        public string PaymentPreLoadHttpFunctionXFunctionKey { get; set; }
    }
}
