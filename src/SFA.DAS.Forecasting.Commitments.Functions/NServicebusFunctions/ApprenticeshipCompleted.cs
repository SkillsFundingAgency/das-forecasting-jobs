using Microsoft.Azure.WebJobs;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.NServicebusFunctions
{
    public static class ApprenticeshipCompleted
    {
        [FunctionName("ApprenticeshipCompleted")]
        public static async Task Run(
            [NServiceBusTrigger(Endpoint = "SFA.DAS.Fcast.ApprenticeshipCompletedEvent")] ApprenticeshipCompletedEvent message)
        {
            await Task.FromResult(0);

        }
    }
}
