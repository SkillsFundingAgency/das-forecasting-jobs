using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Triggers;
using SFA.DAS.Forecasting.Jobs.Infrastructure.Configuration;

namespace SFA.DAS.Forecasting.Jobs.Infrastructure.NServicebus
{
    public class NServiceBusTriggerBindingProvider : ITriggerBindingProvider
    {
        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            var parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<NServiceBusTriggerAttribute>(false);

            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            if (string.IsNullOrEmpty(attribute.Connection))
            {
                attribute.Connection = EnvironmentVariables.NServiceBusConnectionString;
            }

            return Task.FromResult<ITriggerBinding>(new NServiceBusTriggerBinding(parameter, attribute));
        }
    }
}
