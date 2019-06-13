namespace SFA.DAS.Forecasting.Domain.Configuration
{
    public class ForecastingJobsConfiguration
    {
        public string NServiceBusConnectionString { get; set; }
        public string LevyDeclarationPreLoadHttpFunctionBaseUrl { get; set; }
        public string LevyDeclarationPreLoadHttpFunctionXFunctionKey { get; set; }
    }
}
