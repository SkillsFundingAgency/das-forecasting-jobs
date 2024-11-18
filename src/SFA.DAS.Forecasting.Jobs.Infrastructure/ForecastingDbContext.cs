using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Models;

namespace SFA.DAS.Forecasting.Jobs.Infrastructure;

public interface IForecastingDbContext
{
    DbSet<Commitments> Commitment { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class ForecastingDbContext : DbContext, IForecastingDbContext
{
    private const string AzureResource = "https://database.windows.net/";
    private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
    private readonly IConfiguration _configuration;x

    public DbSet<Commitments> Commitment { get; set; }

    public ForecastingDbContext(DbContextOptions options) : base(options)
    {
    }

    public ForecastingDbContext(IConfiguration config, DbContextOptions options, AzureServiceTokenProvider azureServiceTokenProvider) : base(options)
    {
        _configuration = config;
        _azureServiceTokenProvider = azureServiceTokenProvider;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_configuration == null || _azureServiceTokenProvider == null)
        {
            return;
        }

        var connection = new SqlConnection
        {
            ConnectionString = _configuration["DatabaseConnectionString"]!,
            AccessToken = _azureServiceTokenProvider.GetAccessTokenAsync(AzureResource).Result
        };

        optionsBuilder.UseSqlServer(connection);
    }
}