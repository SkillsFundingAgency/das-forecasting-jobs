using System;

namespace SFA.DAS.Forecasting.Domain.Infrastructure;

public interface IServiceProviderBuilder
{
    IServiceProvider Build();
}