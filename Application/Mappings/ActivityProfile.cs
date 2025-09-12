using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings
{
    public class ActivityProfile : Profile
    {
        public ActivityProfile()
        {
            CreateMap<Activity, ActivityResponseDto>();
            CreateMap<ActivityResponseDto, Activity>();
        }
    }
}
