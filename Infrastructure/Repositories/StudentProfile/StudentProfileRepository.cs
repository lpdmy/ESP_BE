using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class StudentProfileRepository : BaseRepository<StudentProfile>, IStudentProfileRepository
    {
        public StudentProfileRepository(EduShpereDbContext context) : base(context)
        {
        }

        public async Task<StudentProfile> CreateOrUpdateStudentProfileAsync(
             StudentProfile profile,
             DateTime? birthDate = null,
             string? phoneNumber = null,
             string? avatarUrl = null)
        {
            var existingProfile = await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == profile.UserId);

            if (existingProfile != null)
            {
                // Update StudentProfile
                existingProfile.UpdatedAt = DateTime.Now;
                existingProfile.UpdatedBy = profile.UserId;
                existingProfile.Bio = profile.Bio;
                existingProfile.ExtraJson = profile.ExtraJson;

                // Update User nếu có dữ liệu mới
                if (!string.IsNullOrEmpty(avatarUrl))
                    existingProfile.User.AvatarUrl = avatarUrl;

                if (birthDate != null)
                    existingProfile.User.Birthdate = birthDate;

                if (!string.IsNullOrEmpty(phoneNumber))
                    existingProfile.User.PhoneNumber = phoneNumber;
            }
            else
            {
                // Tạo mới StudentProfile
                profile.CreatedAt = DateTime.Now;
                profile.CreatedBy = profile.UserId;
                profile.UpdatedAt = DateTime.Now;
                profile.UpdatedBy = profile.UserId;
                await _dbSet.AddAsync(profile);

                // Update User nếu có dữ liệu mới
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == profile.UserId);
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(avatarUrl))
                        user.AvatarUrl = avatarUrl;

                    if (birthDate != null)
                        user.Birthdate = birthDate;

                    if (!string.IsNullOrEmpty(phoneNumber))
                        user.PhoneNumber = phoneNumber;
                }
            }

            await _context.SaveChangesAsync();
            return existingProfile ?? profile;
        }
    }
}
