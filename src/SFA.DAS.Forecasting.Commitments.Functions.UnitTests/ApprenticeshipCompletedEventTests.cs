using AutoFixture;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Commitments.Functions.NServicebusTriggerFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        [Test]
        public async Task ReceivedApprenticeshipCompletedEvent_IfApprenticeshipNotExists_ThenCreateRecord()
        {
            //Arrange
            var fixture = new ApprenticeshipCompletedEventCommitmentsApiTestsFixture();            

            //Act
            await fixture.Run();

            //Assert
            fixture.AssertRecordCreated();         
        }


        [Test]
        public async Task ReceivedApprenticeshipCompletedEvent_IfApprenticeshipNotExists_ThenCreateRecordFails()
        {
            //Arrange
            var fixture = new ApprenticeshipCompletedEventExceptionTestsFixture();

            //Act            
            await fixture.Run();

            //Assert
            fixture.VerifyExceptionLogged();
        }

    }

    public class ApprenticeshipCompletedEventTestsFixture
    {
        public Mock<IMessageHandlerContext> MessageHandlerContext { get; set; }
        public Mock<ICommitmentsApiClient> MockCommitmentsApiClient { get; set; }
        public Mock<IMapper> MockMapper { get; set; }
        public Mock<ILogger<ApprenticeshipCompletedFunction>> MockLogger { get; set; } 
        public ForecastingDbContext Db { get; set; }
        public Commitments Commitment { get; set; }
        public Fixture Fixture { get; set; }
        public long CommitmentId { get; set; }
        public ApprenticeshipCompletedFunction Sut { get; set; }
        public GetApprenticeshipResponse ApprenticeshipResponse { get; set; }

        public ApprenticeshipCompletedEvent ApprenticeshipCompletedEvent { get; set; }

        public ApprenticeshipCompletedEventTestsFixture()
        {
            MessageHandlerContext = new Mock<IMessageHandlerContext>();
            MockCommitmentsApiClient = new Mock<ICommitmentsApiClient>();
            MockMapper = new Mock<IMapper>();
            MockLogger = new Mock<ILogger<ApprenticeshipCompletedFunction>>();
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

            ApprenticeshipCompletedEvent = Fixture.Create<ApprenticeshipCompletedEvent>();
            ApprenticeshipCompletedEvent.ApprenticeshipId = Commitment.ApprenticeshipId;

            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            var mapper = new Mapper(configuration);

            Sut = new ApprenticeshipCompletedFunction(Db, MockCommitmentsApiClient.Object, mapper, MockLogger.Object);
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

        internal void AssertRecordCreated()
        {
            MockLogger.VerifyLogging(LogLevel.Information);
            Assert.AreEqual(1, Db.Commitment.Where(x => x.ApprenticeshipId == ApprenticeshipResponse.Id).Count());
        }

        public void VerifyExceptionLogged()
        {
            MockLogger.VerifyLogging(LogLevel.Information);
            MockLogger.VerifyLogging(LogLevel.Error);         
        }
    }

    public class ApprenticeshipCompletedEventCommitmentsApiTestsFixture : ApprenticeshipCompletedEventTestsFixture
    {
        public ApprenticeshipCompletedEventCommitmentsApiTestsFixture()
        {
            ApprenticeshipCompletedEvent.ApprenticeshipId = 0;
        }
    }

    public class ApprenticeshipCompletedEventExceptionTestsFixture : ApprenticeshipCompletedEventTestsFixture
    {
        public ApprenticeshipCompletedEventExceptionTestsFixture()
        {
            ApprenticeshipCompletedEvent.ApprenticeshipId = 0;
            MockCommitmentsApiClient.Setup(s => s.GetApprenticeship(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                    .Throws(new CommitmentsApiModelException(new List<ErrorDetail>()));
            MockCommitmentsApiClient.Setup(x => x.GetApprenticeship(It.IsAny<long>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            
        }
    }

    public static class VerifyLog
    {
        public static Mock<ILogger<T>> VerifyLogging<T>(this Mock<ILogger<T>> logger, LogLevel level, Times? times = null)
        {
            times ??= Times.Once();

            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == level),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), (Times)times);

            return logger;
        }
    }
}