using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace SFA.DAS.Forecasting.Jobs.Infrastructure.NServicebus
{
    [Extension("NServiceBus")]
    public class NServiceBusExtensionConfig : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            context.AddBindingRule<NServiceBusTriggerAttribute>()
                .BindToTrigger(new NServiceBusTriggerBindingProvider());
        }
    }
}
