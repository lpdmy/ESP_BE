using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;
using EduShpere.Shared.Constants;

namespace EduShpere.Infrastructure.Repositories
{
    public class StudentProfileRepository : BaseRepository<StudentProfile>, IStudentProfileRepository
    {
        public StudentProfileRepository(EduShpereDbContext context) : base(context)
        {
        }

        public async Task<StudentProfile?> GetStudentProfileByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<StudentProfile?> GetStudentProfileByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(p => p.User)
                .Include(p => p.User.ClassGroupMembers)
                    .ThenInclude(cgm => cgm.ClassGroup)
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
        }

        public async Task<IEnumerable<StudentProfile>> GetAllStudentProfilesAsync()
        {
            return await _dbSet
                .Include(p => p.User)
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<StudentProfile> CreateStudentProfileAsync(
            StudentProfile profile,
            DateTime? birthDate = null,
            string? phoneNumber = null,
            string? avatarUrl = null,
            int? classGroupId = null)
        {
            // Kiểm tra xem user đã có profile chưa
            var existingProfile = await _dbSet
                .FirstOrDefaultAsync(p => p.UserId == profile.UserId && !p.IsDeleted);
            
            if (existingProfile != null)
            {
                throw new InvalidOperationException(ErrorMessages.UserProfile.ProfileAlreadyExists);
            }

            // Set audit fields manually in repository
            var now = DateTime.Now;
            profile.CreatedAt = now;
            profile.CreatedBy = profile.UserId;
            profile.UpdatedAt = now;
            profile.UpdatedBy = profile.UserId;
            profile.IsDeleted = false;

            await _dbSet.AddAsync(profile);

            // Cập nhật thông tin User nếu có dữ liệu mới
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

            // Tạo ClassGroupMember nếu có classGroupId
            if (classGroupId.HasValue)
            {
                // Kiểm tra xem user đã có trong class group này chưa
                var existingMember = await _context.ClassGroupMembers
                    .FirstOrDefaultAsync(cgm => cgm.UserId == profile.UserId && cgm.ClassGroupId == classGroupId.Value && !cgm.IsDeleted);
                
                if (existingMember == null)
                {
                    var classGroupMember = new ClassGroupMember
                    {
                        UserId = profile.UserId,
                        ClassGroupId = classGroupId.Value,
                        RowVersion = new byte[8] // Initialize with empty byte array
                    };
                    
                    var memberNow = DateTime.UtcNow;
                    classGroupMember.CreatedAt = memberNow;
                    classGroupMember.CreatedBy = profile.UserId;
                    classGroupMember.UpdatedAt = memberNow;
                    classGroupMember.UpdatedBy = profile.UserId;
                    classGroupMember.IsDeleted = false;
                    
                    await _context.ClassGroupMembers.AddAsync(classGroupMember);
                }
            }

            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task<StudentProfile> UpdateStudentProfileAsync(
            StudentProfile profile,
            DateTime? birthDate = null,
            string? phoneNumber = null,
            string? avatarUrl = null)
        {
            var existingProfile = await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == profile.UserId && !p.IsDeleted);

            if (existingProfile == null)
                throw new KeyNotFoundException(ErrorMessages.UserProfile.ProfileNotFound);

            // Cập nhật StudentProfile
            existingProfile.UpdatedAt = DateTime.UtcNow;
            existingProfile.UpdatedBy = profile.UserId;
            existingProfile.Bio = profile.Bio;
            existingProfile.ExtraJson = profile.ExtraJson;
            existingProfile.StudentNumber = profile.StudentNumber;
            existingProfile.EnrollmentYear = profile.EnrollmentYear;

            // Cập nhật thông tin User nếu có dữ liệu mới
            if (!string.IsNullOrEmpty(avatarUrl))
                existingProfile.User.AvatarUrl = avatarUrl;

            if (birthDate != null)
                existingProfile.User.Birthdate = birthDate;

            if (!string.IsNullOrEmpty(phoneNumber))
                existingProfile.User.PhoneNumber = phoneNumber;

            await _context.SaveChangesAsync();
            return existingProfile;
        }

        public async Task<bool> DeleteStudentProfileAsync(int id)
        {
            var profile = await _dbSet.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (profile == null)
                return false;

            profile.IsDeleted = true;
            profile.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> StudentProfileExistsAsync(int id)
        {
            return await _dbSet.AnyAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<bool> StudentProfileExistsByUserIdAsync(int userId)
        {
            return await _dbSet.AnyAsync(p => p.UserId == userId && !p.IsDeleted);
        }

        public async Task<StudentProfile> AddAsync(StudentProfile profile)
        {
            await _dbSet.AddAsync(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task UpdateAsync(StudentProfile profile)
        {
            _dbSet.Update(profile);
            await _context.SaveChangesAsync();
        }
    }
}
