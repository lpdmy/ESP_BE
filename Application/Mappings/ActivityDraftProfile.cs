using AutoMapper;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Domain.Models;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace EduShpere.Application.Mappings
{
    public class ActivityDraftProfile : Profile
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ActivityDraftProfile()
        {
            // CreateActivityDraftDto to ActivityDraft
            CreateMap<CreateActivityDraftDto, ActivityDraft>()
                .ForMember(dest => dest.Rules, opt => opt.Ignore()) // Handle in service
                .ForMember(dest => dest.SportsCategories, opt => opt.Ignore())
                .ForMember(dest => dest.SportsConfigurations, opt => opt.Ignore())
                .ForMember(dest => dest.Speakers, opt => opt.Ignore())
                .ForMember(dest => dest.ProgramItems, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // UpdateActivityDraftDto to ActivityDraft
            CreateMap<UpdateActivityDraftDto, ActivityDraft>()
                .ForMember(dest => dest.Rules, opt => opt.Ignore())
                .ForMember(dest => dest.SportsCategories, opt => opt.Ignore())
                .ForMember(dest => dest.SportsConfigurations, opt => opt.Ignore())
                .ForMember(dest => dest.Speakers, opt => opt.Ignore())
                .ForMember(dest => dest.ProgramItems, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // ActivityDraft to ActivityDraftResponseDto
            CreateMap<ActivityDraft, ActivityDraftResponseDto>()
                .ForMember(dest => dest.Rules, opt => opt.Ignore()) // Handle in service
                .ForMember(dest => dest.SportsCategories, opt => opt.Ignore())
                .ForMember(dest => dest.SportsConfigurations, opt => opt.Ignore())
                .ForMember(dest => dest.Speakers, opt => opt.Ignore())
                .ForMember(dest => dest.ProgramItems, opt => opt.Ignore());

            // ActivityDraft to ActivityDraftListItemDto
            CreateMap<ActivityDraft, ActivityDraftListItemDto>();
        }
    }
}

