using AutoMapper;
using EduShpere.Domain.Models;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.AuthDto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EduShpere.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
        }
    }
}
