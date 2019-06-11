using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NServiceBus;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.AzureServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;

namespace SFA.DAS.Forecasting.Trigger.TestConsole
{
    public class NServiceBusConsole
    {
        private readonly IConfiguration _configuration;
        public NServiceBusConsole(IConfiguration config)
        {
            _configuration = config;
        }

        public async Task Run()
        {
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.Forecasting.Triggers.TestConsole")
                .UseAzureServiceBusTransport(_configuration.GetConnectionStringOrSetting("NServiceBusConnectionString"), r =>
                {
                    // for testing messages rather than event 
                    // r.RouteToEndpoint(typeof(TestEvent), "TestQueue");
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
                var keyPress = Console.ReadKey();

                if (keyPress.Key == ConsoleKey.Enter)
                {
                    await endpointInstance.Publish(new RefreshEmployerLevyDataCompletedEvent
                    {
                        AccountId = 1,
                        Created = DateTime.Now,
                        LevyImported = true,
                        PeriodMonth = 6,
                        PeriodYear = "19/20"
                    });


                    Console.WriteLine("Message sent...");
                }
            } while (!command.Equals("q"));


            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}
