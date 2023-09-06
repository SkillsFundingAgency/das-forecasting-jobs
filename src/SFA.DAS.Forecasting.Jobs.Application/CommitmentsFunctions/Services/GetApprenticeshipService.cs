using AutoMapper;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Models;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Services;
using SFA.DAS.Forecasting.Jobs.Infrastructure.CosmosDB;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Handlers.Services;

public class GetApprenticeshipService : IGetApprenticeshipService
{
    private readonly IMapper _mapper;
    private readonly ICommitmentsApiClient _commitmentsApiClient;
    private readonly ILogger _logger;
    private readonly IDocumentSession _documentSession;

    public GetApprenticeshipService(ICommitmentsApiClient commitmentsApiClient,
        IMapper mapper,
        IDocumentSession documentSession,
        ILogger<GetApprenticeshipService> logger)
    {
        _commitmentsApiClient = commitmentsApiClient;
        _mapper = mapper;
        _documentSession = documentSession;
        _logger = logger;
    }

    public async Task<Commitments> GetApprenticeshipDetails(long apprenticeshipId)
    {
        _logger.LogDebug($"Get details from approval about apprenticeship Id : {apprenticeshipId}");
        var apprenticeshipResponse = await _commitmentsApiClient.GetApprenticeship(apprenticeshipId);
        var apprenticeshipDetails = _mapper.Map<Commitments>(apprenticeshipResponse);

        _logger.LogDebug($"Get details from cosmos db about course : {apprenticeshipResponse.CourseCode}");
        var courseDetails = await _documentSession.Get<ApprenticeshipCourse>(apprenticeshipResponse.CourseCode);
        apprenticeshipDetails.CourseLevel = courseDetails?.Level ?? 0;

        return apprenticeshipDetails;
    }
}