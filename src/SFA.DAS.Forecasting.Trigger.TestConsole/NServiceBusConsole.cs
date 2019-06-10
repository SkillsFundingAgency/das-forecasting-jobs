using System;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Forecasting.Domain;
using SFA.DAS.Forecasting.Jobs.Infrastructure.Configuration;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.AzureServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;

namespace SFA.DAS.Forecasting.Trigger.TestConsole
{
    public class NServiceBusConsole
    {
        public async Task Run()
        {
            var connectionString = "Endpoint=sb://cofaulco-nsb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=U0LZk6VMV3tuJcLKGfPpsadvPHVhBWTy6qVmgZ/z3KI=";
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.Forecasting.Triggers.TestConsole")
                .UseAzureServiceBusTransport(connectionString, r =>
                {
                    r.RouteToEndpoint(typeof(TestEvent), "TestQueue");
                })
                .UseErrorQueue()
                .UseInstallers()
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            var command = string.Empty;

            do
            {
                Console.WriteLine("Enter 'q' to exit..." + Environment.NewLine);
                Console.Write("Press enter to publish: ");

                await endpointInstance.Publish(new TestEvent { Text = "some text" });

                Console.WriteLine("Message sent...");
            } while (!command.Equals("q"));


            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}
