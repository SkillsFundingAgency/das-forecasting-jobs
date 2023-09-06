using System;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Forecasting.Domain.Infrastructure;

namespace SFA.DAS.Forecasting.Jobs.Infrastructure.DependencyInjection;

internal class ServiceProviderBuilder : IServiceProviderBuilder
{
    private readonly Action<IServiceCollection> _configureServices;

    public ServiceProviderBuilder(Action<IServiceCollection> configureServices) =>
        _configureServices = configureServices;

    public IServiceProvider Build()
    {
        var services = new ServiceCollection();
        _configureServices(services);
        return services.BuildServiceProvider();
    }
}