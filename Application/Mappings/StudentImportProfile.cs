using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Mappings
{
    public class StudentImportProfile : Profile
    {
        public StudentImportProfile()
        {
            // No direct mapping needed as we're using Dictionary<string, string> for student data
            // The mapping is handled manually in the service
        }
    }
}
