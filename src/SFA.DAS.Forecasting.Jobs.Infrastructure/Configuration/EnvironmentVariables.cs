using System;

namespace SFA.DAS.Forecasting.Jobs.Infrastructure.Configuration
{
    public class EnvironmentVariables
    {
        //TODO: Get Connection String
        public static string NServiceBusConnectionString = Environment.GetEnvironmentVariable("NServiceBusConnectionString");
        public static string NServiceBusLicense = Environment.GetEnvironmentVariable("NServiceBusLicense");
    }
}
