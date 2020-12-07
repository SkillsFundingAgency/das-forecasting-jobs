using AutoFixture;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Commitments.Functions.UnitTests
{
    [TestFixture]
    public class ApprenticeshipStoppedEventTests
    {
        [Test]
        public async Task ApprenticeshipComplatedEventUpdatesTheActualEndDate()
        {
            var fixture = new ApprenticeshipStoppedEventTestsFixture(true);
            await fixture.Run();

            fixture.AssertActualEndDate();
        }

        [Test]
        public async Task ApprenticeshipComplatedEventUpdatesTheStatus()
        {
            var fixture = new ApprenticeshipStoppedEventTestsFixture(true);
            await fixture.Run();

            fixture.AssertStatus();
        }


        [Test]
        public async Task ReceivedApprenticeshipStoppedEvent_IfApprenticeshipNotExists_ThenCreateRecord()
        {
            //Arrange
            var fixture = new ApprenticeshipStoppedEventTestsFixture(false);
            
            //Act
            await fixture.Run();

            //Assert
            fixture.AssertRecordCreated();
        }

    }

    public class ApprenticeshipStoppedEventTestsFixture
    {
        public Mock<IMessageHandlerContext> MessageHandlerContext { get; set; }
        public Mock<ICommitmentsApiClient> MockCommitmentsApiClient { get; set; }
        public ForecastingDbContext Db { get; set; }
        public Commitments Commitment { get; set; }
        public Fixture Fixture { get; set; }
        public long CommitmentId { get; set; }
        public ApprenticeshipStoppedFunction Sut { get; set; }
        public GetApprenticeshipResponse ApprenticeshipResponse { get; set; }

        public ApprenticeshipStoppedEvent ApprenticeshipStoppedEvent { get; set; }

        public ApprenticeshipStoppedEventTestsFixture(bool setApprenticeshipId)
        {
            MessageHandlerContext = new Mock<IMessageHandlerContext>();
            MockCommitmentsApiClient = new Mock<ICommitmentsApiClient>();
            Fixture = new Fixture();

            ApprenticeshipResponse = Fixture.Create<GetApprenticeshipResponse>();
            ApprenticeshipResponse.Uln = "12345";
            MockCommitmentsApiClient.Setup(x => x.GetApprenticeship(It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(ApprenticeshipResponse);

            Db = new ForecastingDbContext(new DbContextOptionsBuilder<ForecastingDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .Options);

            Commitment = Fixture.Create<Commitments>();
            Commitment.Id = CommitmentId = 1;
            Commitment.ActualEndDate = null;
            Commitment.Status = Status.LiveOrWaitingToStart;
            Db.Commitment.Add(Commitment);

            ApprenticeshipStoppedEvent = Fixture.Create<ApprenticeshipStoppedEvent>();

            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            var mapper = new Mapper(configuration);

            if (setApprenticeshipId) { ApprenticeshipStoppedEvent.ApprenticeshipId = Commitment.ApprenticeshipId; }
            Sut = new ApprenticeshipStoppedFunction(Db, MockCommitmentsApiClient.Object, mapper);
            Db.SaveChanges();
        }

        public async Task Run()
        {
            await Sut.Run(ApprenticeshipStoppedEvent);
        }

        internal void AssertActualEndDate()
        {
            Assert.AreEqual(ApprenticeshipStoppedEvent.StopDate, Db.Commitment.Where(x => x.Id == CommitmentId).First().ActualEndDate);
        }

        internal void AssertStatus()
        {
            Assert.AreEqual(Status.Stopped, Db.Commitment.Where(x => x.Id == CommitmentId).First().Status);
        }

        internal void AssertRecordCreated()
        {
            Assert.AreEqual(1, Db.Commitment.Where(x => x.ApprenticeshipId == ApprenticeshipResponse.Id).Count());
        }
        
    }
}