using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EduShpere.Application.DTOs.SubmissionDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application
{
    public class SubmissionProfile : Profile
    {
        public SubmissionProfile()
        {
            CreateMap<Submission, SubmissionResponseDto>()
               .ForMember(dest => dest.FirstName, opt => opt.MapFrom(p => p.User.FirstName))
               .ForMember(dest => dest.LastName, opt => opt.MapFrom(p => p.User.LastName))
               .ForMember(dest => dest.Class, opt => opt.MapFrom(p =>
                   p.User.ClassGroupMembers.Select(cgm => cgm.ClassGroup).OrderByDescending(cg => cg.CreatedAt).FirstOrDefault()
               ))
               .ForMember(dest => dest.Users, opt => opt.MapFrom(p =>
    p.JuryAssignments.Select(j => j.UserId).ToList()
))
               .ForMember(dest => dest.Score, opt => opt.MapFrom(p => p.Score))
               .ForMember(dest => dest.ActivityName, opt => opt.MapFrom(p => p.Activity.Title));
            CreateMap<ClassGroup, ClassGroupDto>();
        }
    }
}
