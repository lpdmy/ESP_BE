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
    public class CollectionProfile : Profile
    {
        public CollectionProfile ()
        {
            CreateMap<FavoriteCollection, CollectionResponseDto>()
                .ForMember(dest => dest.CollectionItems,
                    opt => opt.MapFrom(src => src.CollectionItems.Select(ci => ci.PostId)));
        }
    }
}
