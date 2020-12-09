//using AutoFixture;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Diagnostics;
//using Moq;
//using NServiceBus;
//using NUnit.Framework;
//using SFA.DAS.CommitmentsV2.Messages.Events;
//using SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace SFA.DAS.Forecasting.Commitments.Functions.UnitTests
//{
//    [TestFixture]
//    public class ApprenticeshipStoppedEventTests
//    {
//        [Test]
//        public async Task ApprenticeshipComplatedEventUpdatesTheActualEndDate()
//        {
//            var fixture = new ApprenticeshipStoppedEventTestsFixture();
//            await fixture.Run();

//            fixture.AssertActualEndDate();
//        }

//        [Test]
//        public async Task ApprenticeshipComplatedEventUpdatesTheStatus()
//        {
//            var fixture = new ApprenticeshipStoppedEventTestsFixture();
//            await fixture.Run();

//            fixture.AssertStatus();
//        }
//    }

//    public class ApprenticeshipStoppedEventTestsFixture
//    {
//        public Mock<IMessageHandlerContext> MessageHandlerContext { get; set; }
//        public ForecastingDbContext Db { get; set; }
//        public Commitments Commitment { get; set; }
//        public Fixture Fixture { get; set; }
//        public long CommitmentId { get; set; }
//        public ApprenticeshipStoppedFunction Sut { get; set; }

//        public ApprenticeshipStoppedEvent ApprenticeshipStoppedEvent { get; set; }

//        public ApprenticeshipStoppedEventTestsFixture()
//        {
//            MessageHandlerContext = new Mock<IMessageHandlerContext>();
//            Fixture = new Fixture();

//            Db = new ForecastingDbContext(new DbContextOptionsBuilder<ForecastingDbContext>()
//                .UseInMemoryDatabase(Guid.NewGuid().ToString())
//                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
//                .Options);

//            Commitment = Fixture.Create<Commitments>();
//            Commitment.Id = CommitmentId = 1;
//            Commitment.ActualEndDate = null;
//            Commitment.Status = Status.LiveOrWaitingToStart;
//            Db.Commitment.Add(Commitment);

//            ApprenticeshipStoppedEvent = Fixture.Create<ApprenticeshipStoppedEvent>();
//            ApprenticeshipStoppedEvent.ApprenticeshipId = Commitment.ApprenticeshipId;

//            Sut = new ApprenticeshipStoppedFunction(Db);
//            Db.SaveChanges();
//        }

//        public async Task Run()
//        {
//           await Sut.Run(ApprenticeshipStoppedEvent);
//        }

//        internal void AssertActualEndDate()
//        {
//             Assert.AreEqual(ApprenticeshipStoppedEvent.StopDate,  Db.Commitment.Where(x => x.Id == CommitmentId).First().ActualEndDate);
//        }

//        internal void AssertStatus()
//        {
//            Assert.AreEqual(Status.Stopped, Db.Commitment.Where(x => x.Id == CommitmentId).First().Status);
//        }
//    }
//}