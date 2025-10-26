using AutoMapper;
using EduShpere.Application.DTOs.ClassGroupDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings;

public class ClassGroupProfile : Profile
{
    public ClassGroupProfile()
    {
        // Create mapping
        CreateMap<CreateClassGroupDto, ClassGroup>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.ClassGroupMembers, opt => opt.Ignore())
            .ForMember(dest => dest.Posts, opt => opt.Ignore());

        // Update mapping
        CreateMap<UpdateClassGroupDto, ClassGroup>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.ClassGroupMembers, opt => opt.Ignore())
            .ForMember(dest => dest.Posts, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Response mapping
        CreateMap<ClassGroup, ClassGroupDto>()
            .ForMember(dest => dest.CurrentStudentCount, opt => opt.MapFrom(src => 
                src.ClassGroupMembers != null ? src.ClassGroupMembers.Count(m => !m.IsDeleted) : 0))
            .ForMember(dest => dest.AcademicYearName, opt => opt.MapFrom(src => src.AcademicYears != null ? src.AcademicYears.Name : null));

        // Academic Year mapping
        CreateMap<AcademicYear, AcademicYearDto>();
    }
}
