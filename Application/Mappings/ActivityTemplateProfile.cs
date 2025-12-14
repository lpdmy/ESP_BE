using AutoMapper;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings;

public class ActivityTemplateProfile : Profile
{
    public ActivityTemplateProfile()
    {
        // ActivityTemplate to DTO (mapping được xử lý trong Service vì cần deserialize JSON)
        // Chỉ map các field đơn giản, JSON fields được xử lý trong Service
        CreateMap<ActivityTemplate, ActivityTemplateDto>()
            .ForMember(dest => dest.PrefillData, opt => opt.Ignore())
            .ForMember(dest => dest.Checklist, opt => opt.Ignore());
    }
}

