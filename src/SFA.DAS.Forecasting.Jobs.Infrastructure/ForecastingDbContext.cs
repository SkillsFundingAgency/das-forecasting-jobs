using System.Threading;
using System.Threading.Tasks;
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
    private readonly IConfiguration _configuration;

    public DbSet<Commitments> Commitment { get; set; }

    public ForecastingDbContext(DbContextOptions options) : base(options)
    {
    }

    public ForecastingDbContext(IConfiguration config, DbContextOptions options) : base(options)
    {
        _configuration = config;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_configuration == null)
        {
            return;
        }

        var connection = new SqlConnection
        {
            ConnectionString = _configuration["DatabaseConnectionString"]!,
        };

        optionsBuilder.UseSqlServer(connection);
    }
}