﻿using AutoFixture;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Models;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Services;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Handlers;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Mapper;
using SFA.DAS.Forecasting.Jobs.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace SFA.DAS.Forecasting.Jobs.Application.UnitTests.Handlers;

public class WhenHandlingApprenticeshipStoppedEvent
{
    [Test]
    public async Task Then_Update_ActualEndDate()
    {
        //Arrange
        var fixture = new ApprenticeshipStoppedEventTestsFixture();

        //Act
        await fixture.Run();

        //Assert
        fixture.AssertActualEndDate();
    }

    [Test]
    public async Task Then_Update_Status()
    {
        //Arrange
        var fixture = new ApprenticeshipStoppedEventTestsFixture();

        //Act
        await fixture.Run();

        //Assert
        fixture.AssertStatus();
    }

    [Test]
    public async Task If_Apprenticeship_NotExists_Then_CreateRecord()
    {
        //Arrange
        var fixture = new ApprenticeshipStoppedEventTestsFixture().SetGetApprenticeshipService();

        //Act
        await fixture.Run();

        //Assert
        fixture.AssertRecordCreated();
    }


    [Test]
    public void If_Event_Errors_Should_Log_Error()
    {
        //Arrange            
        var fixture = new ApprenticeshipStoppedEventTestsFixture().SetGetApprenticeshipService().SetException();

        //Act
        fixture.RunEventWithException();

        //Assert
        fixture.VerifyExceptionLogged();
    }

    [Test]
    public void If_Api_Call_Unsuccesful_Should_Log_Error()
    {
        //Arrange            
        var fixture = new ApprenticeshipStoppedEventTestsFixture().SetGetApprenticeshipService().SetCommitmentsApiModelException();

        //Act
        fixture.RunEventWithCommitmentsApiModelException();

        //Assert
        fixture.VerifyCommitmentsApiModelExceptionExceptionLogged();
    }
}
public class ApprenticeshipStoppedEventTestsFixture
{
    public Mock<IMessageHandlerContext> MessageHandlerContext { get; set; }
    public Mock<IGetApprenticeshipService> MockGetApprenticeship { get; set; }
    public Mock<ILogger<ApprenticeshipStoppedEventHandler>> MockLogger { get; set; }
    public Mock<IApprenticeshipStoppedEventHandler> MockApprenticeshipCompletionDateUpdatedEventHandler { get; set; }
    public ForecastingDbContext Db { get; set; }
    public Commitments Commitment { get; set; }
    public Fixture Fixture { get; set; }
    public long CommitmentId { get; set; }
    public ApprenticeshipStoppedEventHandler Sut { get; set; }

    public ApprenticeshipStoppedEvent ApprenticeshipStoppedEvent { get; set; }

    public ApprenticeshipStoppedEventTestsFixture()
    {
        MessageHandlerContext = new Mock<IMessageHandlerContext>();
        MockGetApprenticeship = new Mock<IGetApprenticeshipService>();
        MockLogger = new Mock<ILogger<ApprenticeshipStoppedEventHandler>>();
        MockApprenticeshipCompletionDateUpdatedEventHandler = new Mock<IApprenticeshipStoppedEventHandler>();
        Fixture = new Fixture();

        Db = new ForecastingDbContext(new DbContextOptionsBuilder<ForecastingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options);
        
        Commitment = Fixture.Create<Commitments>();
        Commitment.Id = CommitmentId = 101;
        Commitment.ActualEndDate = null;
        Commitment.Status = Status.LiveOrWaitingToStart;
        Commitment.ApprenticeshipId = 1;
        Db.Commitment.Add(Commitment);

        ApprenticeshipStoppedEvent = Fixture.Create<ApprenticeshipStoppedEvent>();
        ApprenticeshipStoppedEvent.ApprenticeshipId = Commitment.ApprenticeshipId;

        Sut = new ApprenticeshipStoppedEventHandler(Db, MockGetApprenticeship.Object, MockLogger.Object);
        Db.SaveChanges();
    }

    public ApprenticeshipStoppedEventTestsFixture SetGetApprenticeshipService()
    {
        ApprenticeshipStoppedEvent.ApprenticeshipId = 2;
        Commitment = Fixture.Create<Commitments>();
        Commitment.Id = 0;
        Commitment.ApprenticeshipId = 2;
        MockGetApprenticeship.Setup(x => x.GetApprenticeshipDetails(It.IsAny<long>())).ReturnsAsync(Commitment);
        
        return this;
    }

    public ApprenticeshipStoppedEventTestsFixture SetCommitmentsApiModelException()
    {
        MockGetApprenticeship.Setup(s => s.GetApprenticeshipDetails(It.IsAny<long>()))
            .Throws(new CommitmentsApiModelException([]));

        return this;
    }

    public ApprenticeshipStoppedEventTestsFixture SetException()
    {
        MockGetApprenticeship.Setup(x => x.GetApprenticeshipDetails(It.IsAny<long>())).ThrowsAsync(new Exception());

        return this;
    }

    public async Task Run()
    {
        await Sut.Handle(ApprenticeshipStoppedEvent);
    }

    public void RunEventWithException()
    {
        var action = () => Sut.Handle(ApprenticeshipStoppedEvent);
        action.Should().ThrowAsync<Exception>();
    }

    public void RunEventWithCommitmentsApiModelException()
    {
        var action = () => Sut.Handle(ApprenticeshipStoppedEvent);
        action.Should().ThrowAsync<CommitmentsApiModelException>();
    }

    internal void AssertActualEndDate()
    {
        Db.Commitment.First(x => x.Id == CommitmentId).ActualEndDate.Should().Be(ApprenticeshipStoppedEvent.StopDate);
    }

    internal void AssertStatus()
    {
        Db.Commitment.First(x => x.Id == CommitmentId).Status.Should().Be(Status.Stopped);
    }

    internal void AssertRecordCreated()
    {
        Db.Commitment.Count(x => x.ApprenticeshipId == 2).Should().Be(1);
    }

    internal void VerifyExceptionLogged()
    {
        MockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                (Func<object, Exception, string>)It.IsAny<object>()), Times.AtLeastOnce());
    }

    internal void VerifyCommitmentsApiModelExceptionExceptionLogged()
    {
        MockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<CommitmentsApiModelException>(),
                (Func<object, Exception, string>)It.IsAny<object>()), Times.AtLeastOnce());
    }
}