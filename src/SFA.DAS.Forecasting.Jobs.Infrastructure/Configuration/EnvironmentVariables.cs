using System;

namespace SFA.DAS.Forecasting.Jobs.Infrastructure.Configuration
{
    public class EnvironmentVariables
    {
        public static string NServiceBusConnectionString = "Endpoint=sb://cofaulco-nsb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=U0LZk6VMV3tuJcLKGfPpsadvPHVhBWTy6qVmgZ/z3KI="; // Environment.GetEnvironmentVariable("NServiceBusConnectionString");
        public static string NServiceBusLicense = Environment.GetEnvironmentVariable("NServiceBusLicense");
    }
}
