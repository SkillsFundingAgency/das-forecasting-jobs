using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Forecasting.Jobs.Infrastructure;
using System;

namespace SFA.DAS.Forecasting.Commitments.Functions.AppStart;

public static class AddDatabaseExtension
{
    public static void AddDatabaseRegistration(this IServiceCollection services, IConfiguration config, string environmentName)
    {
        if (environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
            environmentName.Equals("DEV", StringComparison.CurrentCultureIgnoreCase))
        {
            services.AddDbContext<ForecastingDbContext>(options => options.UseSqlServer(config["DatabaseConnectionString"]), ServiceLifetime.Transient);
        }
        else
        {
            services.AddSingleton(new AzureServiceTokenProvider());
            services.AddDbContext<ForecastingDbContext>(ServiceLifetime.Transient);
        }

        services.AddScoped<IForecastingDbContext, ForecastingDbContext>(provider => provider.GetService<ForecastingDbContext>());
    }
}