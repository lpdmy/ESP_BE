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
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<Comment, CommentResponseDto>()
    .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src =>src.User.AvatarUrl ))
    .ForMember(dest=> dest.UserName,opt => opt.MapFrom(src => src.User != null ? src.User.LastName + " " + src.User.FirstName : null)) ;
        }
    }
}
