using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Forecasting.Domain;
using SFA.DAS.Forecasting.Jobs.Infrastructure.NServicebus;

namespace SFA.DAS.Forecasting.Triggers
{
    public static class TestFunction
    {
        [FunctionName("TestFunction")]
        public static void Run([NServiceBusTrigger(EndPoint = "SFA.DAS.Forecasting.Jobs.TestEvent")]TestEvent mySbMsg, ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg}");
        }
    }
}
