using EduShpere.Domain;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IStudentProfileRepository
    {
        Task<StudentProfile> CreateOrUpdateStudentProfileAsync(StudentProfile profile, DateTime? birthDate = null, string? phoneNumber = null, string? avatarUrl = null);
    }
}
