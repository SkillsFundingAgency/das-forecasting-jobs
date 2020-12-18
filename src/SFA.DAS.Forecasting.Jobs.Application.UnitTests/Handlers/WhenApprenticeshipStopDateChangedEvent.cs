using AutoFixture;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Handlers;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Mapper;
using SFA.DAS.Forecasting.Jobs.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
    
namespace SFA.DAS.Forecasting.Jobs.Application.UnitTests.Handlers
{
    [TestFixture]
    public class WhenApprenticeshipStopDateChangedEvent
    {

        [Test]
        public async Task Then_Update_ActualEndDate()
        {
            //Arrange
            var fixture = new ApprenticeshipStopDateChangedEventFixture();
            
            //Act
            await fixture.Run();

            //Assert
            fixture.AssertActualEndDate();
        }

        [Test]
        public async Task Then_Update_Status()
        {
            //Arrange
            var fixture = new ApprenticeshipStopDateChangedEventFixture();
            
            //Act
            await fixture.Run();

            //Assert
            fixture.AssertStatus();
        }

        [Test]
        public async Task If_Apprenticeship_NotExists_Then_CreateRecord()
        {
            //Arrange
            var fixture = new ApprenticeshipStopDateChangedEventFixture().SetApprenticeshipId();

            //Act
            await fixture.Run();

            //Assert
            fixture.AssertRecordCreated();
        }       


        [Test]
        public void If_Event_Errors_Should_Log_Error()
        {
            //Arrange            
            var fixture = new ApprenticeshipStopDateChangedEventFixture().SetApprenticeshipId().SetException();

            //Act
            fixture.RunEventWithException();

            //Assert
            fixture.VerifyExceptionLogged();
        }

        [Test]
        public void If_Api_Call_Unsuccesful_Should_Log_Error()
        {
            //Arrange            
            var fixture = new ApprenticeshipStopDateChangedEventFixture().SetApprenticeshipId().SetCommitmentsApiModelException();

            //Act
            fixture.RunEventWithCommitmentsApiModelException();

            //Assert
            fixture.VerifyCommitmentsApiModelExceptionExceptionLogged();
        }
    }

    public class ApprenticeshipStopDateChangedEventFixture
    {
        public Mock<IMessageHandlerContext> MessageHandlerContext { get; set; }
        public Mock<ICommitmentsApiClient> MockCommitmentsApiClient { get; set; }
        public Mock<IMapper> MockMapper { get; set; }
        public Mock<ILogger<ApprenticeshipStopDateChangedEventHandler>> MockLogger { get; set; }
        public Mock<IApprenticeshipStopDateChangedEventHandler> MockApprenticeshipStopDateChangedEventHandler { get; set; }
        public ForecastingDbContext Db { get; set; }
        public Commitments Commitment { get; set; }
        public Fixture Fixture { get; set; }
        public long CommitmentId { get; set; }
        public ApprenticeshipStopDateChangedEventHandler Sut { get; set; }
        public GetApprenticeshipResponse ApprenticeshipResponse { get; set; }

        public ApprenticeshipStopDateChangedEvent ApprenticeshipStopDateChangedEvent { get; set; }

        public ApprenticeshipStopDateChangedEventFixture()
        {
            MessageHandlerContext = new Mock<IMessageHandlerContext>();
            MockCommitmentsApiClient = new Mock<ICommitmentsApiClient>();
            MockMapper = new Mock<IMapper>();
            MockLogger = new Mock<ILogger<ApprenticeshipStopDateChangedEventHandler>>();
            MockApprenticeshipStopDateChangedEventHandler = new Mock<IApprenticeshipStopDateChangedEventHandler>();
            Fixture = new Fixture();

            ApprenticeshipResponse = Fixture.Create<GetApprenticeshipResponse>();
            ApprenticeshipResponse.Uln = "12345";
            MockCommitmentsApiClient.Setup(x => x.GetApprenticeship(It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(ApprenticeshipResponse);

            Db = new ForecastingDbContext(new DbContextOptionsBuilder<ForecastingDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())          
                .Options);

            Commitment = Fixture.Create<Commitments>();
            Commitment.Id = CommitmentId = 1;
            Commitment.ActualEndDate = null;
            Commitment.Status = Status.LiveOrWaitingToStart;
            Db.Commitment.Add(Commitment);

            ApprenticeshipStopDateChangedEvent = Fixture.Create<ApprenticeshipStopDateChangedEvent>();
            ApprenticeshipStopDateChangedEvent.ApprenticeshipId = Commitment.ApprenticeshipId;

            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            var mapper = new Mapper(configuration);

            Sut = new ApprenticeshipStopDateChangedEventHandler(Db, MockCommitmentsApiClient.Object, mapper, MockLogger.Object);
            Db.SaveChanges();
        }

        public ApprenticeshipStopDateChangedEventFixture SetApprenticeshipId()
        {
            ApprenticeshipStopDateChangedEvent.ApprenticeshipId = 0;

            return this;
        }
        public ApprenticeshipStopDateChangedEventFixture SetCommitmentsApiModelException()
        {
            MockCommitmentsApiClient.Setup(s => s.GetApprenticeship(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                    .Throws(new CommitmentsApiModelException(new List<ErrorDetail>()));

            return this;
        }

        public ApprenticeshipStopDateChangedEventFixture SetException()
        {
            MockCommitmentsApiClient.Setup(x => x.GetApprenticeship(It.IsAny<long>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            return this;
        }

        public async Task Run()
        {
            await Sut.Handle(ApprenticeshipStopDateChangedEvent);
        }

        public void RunEventWithException()
        {
           Assert.ThrowsAsync<Exception>(() => Sut.Handle(ApprenticeshipStopDateChangedEvent));
        }

        public void RunEventWithCommitmentsApiModelException()
        {
            Assert.ThrowsAsync<CommitmentsApiModelException>(() => Sut.Handle(ApprenticeshipStopDateChangedEvent));
        }

        internal void AssertActualEndDate()
        {
            Assert.AreEqual(ApprenticeshipStopDateChangedEvent.StopDate, Db.Commitment.Where(x => x.Id == CommitmentId).First().ActualEndDate);
        }

        internal void AssertStatus()
        {
            Assert.AreEqual(Status.Stopped, Db.Commitment.Where(x => x.Id == CommitmentId).First().Status);
        }

        internal void AssertRecordCreated()
        {
            
            Assert.AreEqual(1, Db.Commitment.Where(x => x.ApprenticeshipId == ApprenticeshipResponse.Id).Count());
        }       

        internal void VerifyExceptionLogged()
        {
            MockLogger.Verify(
             x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                 It.IsAny<Func<object, Exception, string>>()), Times.AtLeastOnce());
        }

        internal void VerifyCommitmentsApiModelExceptionExceptionLogged()
        {
            MockLogger.Verify(
             x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<CommitmentsApiModelException>(),
                 It.IsAny<Func<object, Exception, string>>()), Times.AtLeastOnce());
        }
    }    

}
