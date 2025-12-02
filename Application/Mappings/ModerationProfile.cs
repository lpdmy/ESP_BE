using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings
{
    public class ModerationProfile : Profile
    {
        public ModerationProfile()
        {
            CreateMap<ReportedContent, ModerationResponseDto>()
    .ForMember(dest => dest.AuthorName,
        opt => opt.MapFrom(src =>
            src.Author != null
                ? $"{src.Author.LastName} {src.Author.FirstName}"
                : null))
    .AfterMap((src, dest) =>
    {
        if (src.ReporterId == 0)
            dest.ReporterName = "Hệ thống";
        else if (src.Reporter != null)
            dest.ReporterName = $"{src.Reporter.LastName} {src.Reporter.FirstName}";
        else
            dest.ReporterName = "Không rõ";
    });
        }
    }
}
