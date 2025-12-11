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
            .ForMember(dest => dest.AcademicYearName, opt => opt.MapFrom(src => src.AcademicYears != null ? src.AcademicYears.Name : null))
            .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => 
                src.Schedules != null ? src.Schedules.Where(s => !s.IsDeleted).OrderBy(s => s.DayOfWeek).ThenBy(s => s.Period).ToList() : null));

        // Schedule mappings
        CreateMap<ClassGroupSchedule, ClassGroupScheduleDto>();
        CreateMap<CreateClassGroupScheduleDto, ClassGroupSchedule>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ClassGroupId, opt => opt.Ignore())
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => ParseTimeSpan(src.StartTime)))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => ParseTimeSpan(src.EndTime)))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.ClassGroup, opt => opt.Ignore());

        // Academic Year mapping
        CreateMap<AcademicYear, AcademicYearDto>();
    }

    private static TimeSpan ParseTimeSpan(string? timeString)
    {
        if (string.IsNullOrWhiteSpace(timeString))
            return TimeSpan.Zero;
        
        if (TimeSpan.TryParse(timeString, out TimeSpan result))
            return result;
        
        return TimeSpan.Zero;
    }
}
