using System;
using Microsoft.Azure.WebJobs.Description;

namespace SFA.DAS.Forecasting.Jobs.Infrastructure.NServicebus
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public sealed class NServiceBusTriggerAttribute : Attribute
    {
        public string EndPoint { get; set; }
        public string Connection { get; set; }
    }
}
