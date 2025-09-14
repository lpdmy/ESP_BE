using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.AuthDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings
{
    public class ActivityParticipantProfile : Profile
    {
        public ActivityParticipantProfile()
        {
            CreateMap<ActivityParticipant, ActivityParticipantResponseDto>();
            CreateMap<ActivityParticipantResponseDto, ActivityParticipant>();
        }
    }
}