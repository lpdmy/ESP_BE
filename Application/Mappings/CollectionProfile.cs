using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CollectionDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings
{
    public class CollectionProfile : Profile
    {
        public CollectionProfile ()
        {
            CreateMap<Post, PostIteamDto>()
            .ForMember(dest => dest.Image,
                opt => opt.MapFrom(src =>
                    src.Attachments.FirstOrDefault().FileUrl))
            .ForMember(dest => dest.Title,
                opt => opt.MapFrom(src => src.Title));

            CreateMap<FavoriteCollection, CollectionResponseDto>()
                .ForMember(dest => dest.CollectionItems,
                    opt => opt.MapFrom(src =>
                        src.CollectionItems.Select(ci => ci.Post)))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        }
    }
}
