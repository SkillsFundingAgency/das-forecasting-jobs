using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.UnitTests
{
    [TestFixture]
    public class ApprenticeshipCompletedEventTests
    {
        [Test]
        public async Task ApprenticeshipComplatedEventUpdatesTheActualEndDate()
        {
            var fixture = new ApprenticeshipCompletedEventTestsFixture();
            await fixture.Run();

            fixture.AssertActualEndDate();
        }

        [Test]
        public async Task ApprenticeshipComplatedEventUpdatesTheStatus()
        {
            var fixture = new ApprenticeshipCompletedEventTestsFixture();
            await fixture.Run();

            fixture.AssertStatus();
        }
    }

    public class ApprenticeshipCompletedEventTestsFixture
    {
        public Mock<IMessageHandlerContext> MessageHandlerContext { get; set; }
        public ForecastingDbContext Db { get; set; }
        public Commitments Commitment { get; set; }
        public Fixture Fixture { get; set; }
        public long CommitmentId { get; set; }
        public ApprenticeshipCompletedFunction Sut { get; set; }

        public ApprenticeshipCompletedEvent ApprenticeshipCompletedEvent { get; set; }

        public ApprenticeshipCompletedEventTestsFixture()
        {
            MessageHandlerContext = new Mock<IMessageHandlerContext>();
            Fixture = new Fixture();

            Db = new ForecastingDbContext(new DbContextOptionsBuilder<ForecastingDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .Options);

            Commitment = Fixture.Create<Commitments>();
            Commitment.Id = CommitmentId = 1;
            Commitment.ActualEndDate = null;
            Commitment.Status = Status.LiveOrWaitingToStart;
            Db.Commitment.Add(Commitment);

            ApprenticeshipCompletedEvent = Fixture.Create<ApprenticeshipCompletedEvent>();
            ApprenticeshipCompletedEvent.ApprenticeshipId = Commitment.ApprenticeshipId;

            Sut = new ApprenticeshipCompletedFunction(Db);
            Db.SaveChanges();
        }

        public async Task Run()
        {
           await Sut.Run(ApprenticeshipCompletedEvent);
        }

        internal void AssertActualEndDate()
        {
             Assert.AreEqual(ApprenticeshipCompletedEvent.CompletionDate,  Db.Commitment.Where(x => x.Id == CommitmentId).First().ActualEndDate);
        }

        internal void AssertStatus()
        {
            Assert.AreEqual(Status.Completed, Db.Commitment.Where(x => x.Id == CommitmentId).First().Status);
        }
    }
}