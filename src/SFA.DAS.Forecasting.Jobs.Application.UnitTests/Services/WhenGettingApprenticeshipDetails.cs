using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Models;
using SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Handlers.Services;
using SFA.DAS.Forecasting.Jobs.Infrastructure.CosmosDB;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;

namespace SFA.DAS.Forecasting.Jobs.Application.UnitTests.Services;

[TestFixture]
public class WhenGettingApprenticeshipDetails
{
    [Test]
    public async Task Should_Call_CommitmentsApi_Once()
    {
        var fixture = new WhenGettingApprenticeshipDetailsFixture();

        await fixture.GetApprenticeshipDetails();

        fixture.VeriyfCommitmentsApiCalledOnce();
    }

    [Test]
    public async Task Should_Call_AutoMapper_Called_Once()
    {
        var fixture = new WhenGettingApprenticeshipDetailsFixture();

        await fixture.GetApprenticeshipDetails();

        fixture.VeriyfAutoMapperCalledOnce();
    }

    [Test]
    public async Task Should_Call_DocumentStore_Once()
    {
        var fixture = new WhenGettingApprenticeshipDetailsFixture();

        await fixture.GetApprenticeshipDetails();

        fixture.VeriyfDocumentStoreCalledOnce();
    }

    [Test]
    public async Task Should_Set_CourseLevel()
    {
        var fixture = new WhenGettingApprenticeshipDetailsFixture();

        var result = await fixture.GetApprenticeshipDetails();

        fixture.ApprenticeshipCourse.Level.Should().Be(result.CourseLevel);
    }


    public class WhenGettingApprenticeshipDetailsFixture
    {
        public GetApprenticeshipService Sut { get; set; }
        public Mock<ICommitmentsApiClient> MockCommitmentsApiClient { get; set; }
        public Mock<IDocumentSession> MockDocumentSession { get; set; }
        public Mock<IMapper> MockMapper { get; set; }
        public long ApprenticeshipId { get; set; }
        public Commitments Commitments { get; set; }
        public GetApprenticeshipResponse GetApprenticeshipResponse { get; set; }
        public ApprenticeshipCourse ApprenticeshipCourse { get; set; }

        public WhenGettingApprenticeshipDetailsFixture()
        {
            var fixture = new Fixture();
            ApprenticeshipId = fixture.Create<long>();
            Commitments = fixture.Create<Commitments>();
            GetApprenticeshipResponse = fixture.Create<GetApprenticeshipResponse>();
            ApprenticeshipCourse = fixture.Create<ApprenticeshipCourse>();

            MockMapper = new Mock<IMapper>();
            MockMapper.Setup(m => m.Map<Commitments>(It.IsAny<GetApprenticeshipResponse>())).Returns(Commitments); // mapping data

            MockDocumentSession = new Mock<IDocumentSession>();
            MockDocumentSession.Setup(m => m.Get<ApprenticeshipCourse>(It.IsAny<string>())).ReturnsAsync(ApprenticeshipCourse);

            MockCommitmentsApiClient = new Mock<ICommitmentsApiClient>();
            MockCommitmentsApiClient.Setup(x => x.GetApprenticeship(It.IsAny<long>(), CancellationToken.None)).ReturnsAsync(GetApprenticeshipResponse);

            Sut = new GetApprenticeshipService(MockCommitmentsApiClient.Object, MockMapper.Object, MockDocumentSession.Object, Mock.Of<ILogger<GetApprenticeshipService>>());
        }

        public Task<Commitments> GetApprenticeshipDetails()
        {
            return Sut.GetApprenticeshipDetails(ApprenticeshipId);
        }

        internal void VeriyfCommitmentsApiCalledOnce()
        {
            MockCommitmentsApiClient.Verify(x => x.GetApprenticeship(ApprenticeshipId, CancellationToken.None), Times.Once);
        }

        internal void VeriyfAutoMapperCalledOnce()
        {
            MockMapper.Verify(x => x.Map<Commitments>(GetApprenticeshipResponse), Times.Once);
        }

        internal void VeriyfDocumentStoreCalledOnce()
        {
            MockDocumentSession.Verify(x => x.Get<ApprenticeshipCourse>(GetApprenticeshipResponse.CourseCode), Times.Once);
        }
    }
}