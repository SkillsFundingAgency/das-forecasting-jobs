﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions
{
    public interface IForecastingDbContext
    {
      //  DbSet<Commitments> Commitment { get; set; }
        Task<int> SaveChangesAsync(long apprenticeshipId, Status status, DateTime actualEndDate);
    }

    public class ForecastingDbContext : IForecastingDbContext
    {
        private string _connectionString;

        public ForecastingDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        //public ForecastingDbContext(DbContextOptions options) : base(options)
        //{
        //}

        // public DbSet<Commitments> Commitment { get; set; }
        public async Task<int> SaveChangesAsync(long apprenticeshipId, Status status, DateTime actualEndDate)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                return await connection.ExecuteAsync($"UPDATE Commitment SET Status = {status} ActualEndDate = {actualEndDate} WHERE ApprenticeshipId = {apprenticeshipId}");
            }
        }
    }

    public class Commitments
    {
        public long Id { get; set; }
        public long EmployerAccountId { get; set; }
        public long SendingEmployerAccountId { get; set; }
        public long ApprenticeshipId { get; set; }
        public long LearnerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal CompletionAmount { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public short NumberOfInstallments { get; set; }
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string ApprenticeName { get; set; }
        public string CourseName { get; set; }
        public int? CourseLevel { get; set; }
        public FundingSource FundingSource { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public bool HasHadPayment { get; set; }
        public Status? Status { get; set; }
    }

    public enum FundingSource : byte
    {
        Levy = 1,
        Transfer = 2,
        CoInvestedSfa = 3,
        CoInvestedEmployer = 4,
        FullyFundedSfa = 5
    }

    public enum Status
    {
        LiveOrWaitingToStart = 0,
        Stopped = 1,
        Completed = 2
    }
}
