using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EduShpere.Application.DTOs.PostDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings
{
    public class AttachmentProfile : Profile
    {
        public AttachmentProfile()
        {
            CreateMap<Attachment, PostAttachmentDto>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.FileUrl))
                .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => src.FileType));
        }
    }
}
