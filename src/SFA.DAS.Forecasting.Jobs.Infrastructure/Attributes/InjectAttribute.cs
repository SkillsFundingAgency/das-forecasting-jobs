using Microsoft.Azure.WebJobs.Description;
using System;

namespace SFA.DAS.Forecasting.Jobs.Infrastructure.Attributes;

[Binding]
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class InjectAttribute : Attribute
{
}