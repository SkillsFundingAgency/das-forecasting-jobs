using AutoMapper;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.Forecasting.Domain.CommitmentsFunctions.Models;
using SFA.DAS.Forecasting.Jobs.Infrastructure;
using System;

namespace SFA.DAS.Forecasting.Jobs.Application.CommitmentsFunctions.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<GetApprenticeshipResponse, Commitments>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.LearnerId, m => m.MapFrom(src => long.Parse(src.Uln)))
            .ForMember(dest => dest.SendingEmployerAccountId, m => m.MapFrom(src => src.EmployerAccountId))
            .ForMember(dest => dest.PlannedEndDate, m => m.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.ApprenticeName, m => m.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.ApprenticeshipId, m => m.MapFrom(src => src.Id))
            .BeforeMap((s, dest) => dest.FundingSource = FundingSource.Levy)
            .BeforeMap((s, dest) => dest.UpdatedDateTime = DateTime.UtcNow)
            .BeforeMap((s, dest) => dest.CourseLevel = 0)
            .BeforeMap((s, dest) => dest.HasHadPayment = false)
            .BeforeMap((s, dest) => dest.MonthlyInstallment = 0)
            .BeforeMap((s, dest) => dest.NumberOfInstallments = 0)
            .BeforeMap((s, dest) => dest.CompletionAmount = 0);
    }
}