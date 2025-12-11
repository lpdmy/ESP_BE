using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using TeacherProfileEntity = EduShpere.Domain.Models.TeacherProfile;

namespace EduShpere.Infrastructure.Repositories.TeacherProfile
{
    public class TeacherProfileRepository : ITeacherProfileRepository
    {
        private readonly EduShpereDbContext _context;
        private readonly DbSet<TeacherProfileEntity> _dbSet;

        public TeacherProfileRepository(EduShpereDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TeacherProfileEntity>();
        }

        public async Task<TeacherProfileEntity> AddAsync(TeacherProfileEntity profile)
        {
            await _dbSet.AddAsync(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task UpdateAsync(TeacherProfileEntity profile)
        {
            _dbSet.Update(profile);
            await _context.SaveChangesAsync();
        }

        public async Task<TeacherProfileEntity?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<TeacherProfileEntity?> GetTeacherProfileByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == id && !p.IsDeleted);
        }

        public async Task<TeacherProfileEntity?> GetTeacherProfileByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
        }

        public async Task<IEnumerable<TeacherProfileEntity>> GetAllTeacherProfilesAsync()
        {
            return await _dbSet
                .Include(p => p.User)
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<TeacherProfileEntity> CreateTeacherProfileAsync(
            TeacherProfileEntity profile,
            DateTime? birthDate = null,
            string? phoneNumber = null,
            string? avatarUrl = null)
        {
            // Kiểm tra xem user đã có profile chưa
            var existingProfile = await _dbSet
                .FirstOrDefaultAsync(p => p.UserId == profile.UserId && !p.IsDeleted);
            
            if (existingProfile != null)
            {
                throw new InvalidOperationException(ErrorMessages.UserProfile.ProfileAlreadyExists);
            }

            // Kiểm tra xem user có tồn tại không
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == profile.UserId && !u.IsDeleted);
            
            if (user == null)
            {
                throw new KeyNotFoundException(ErrorMessages.UserProfile.UserNotFound);
            }

            // Cập nhật thông tin User nếu có
            if (birthDate != null)
                user.Birthdate = birthDate;
            if (!string.IsNullOrEmpty(phoneNumber))
                user.PhoneNumber = phoneNumber;
            if (!string.IsNullOrEmpty(avatarUrl))
                user.AvatarUrl = avatarUrl;

            await _context.SaveChangesAsync();

            // Tạo profile
            profile.CreatedAt = DateTime.UtcNow;
            profile.UpdatedAt = DateTime.UtcNow;
            profile.IsDeleted = false;

            await _dbSet.AddAsync(profile);
            await _context.SaveChangesAsync();

            return profile;
        }

        public async Task<TeacherProfileEntity> UpdateTeacherProfileAsync(
            TeacherProfileEntity profile,
            DateTime? birthDate = null,
            string? phoneNumber = null,
            string? avatarUrl = null)
        {
            var existingProfile = await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == profile.Id && !p.IsDeleted);
            
            if (existingProfile == null)
            {
                throw new KeyNotFoundException(ErrorMessages.UserProfile.ProfileNotFound);
            }

            // Cập nhật thông tin User nếu có
            if (birthDate != null)
                existingProfile.User.Birthdate = birthDate;
            if (!string.IsNullOrEmpty(phoneNumber))
                existingProfile.User.PhoneNumber = phoneNumber;
            if (!string.IsNullOrEmpty(avatarUrl))
                existingProfile.User.AvatarUrl = avatarUrl;

            // Cập nhật thông tin profile
            existingProfile.TeacherCode = profile.TeacherCode;
            existingProfile.Department = profile.Department;
            existingProfile.Position = profile.Position;
            existingProfile.Bio = profile.Bio;
            existingProfile.ExtraJson = profile.ExtraJson;
            existingProfile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingProfile;
        }

        public async Task<bool> DeleteTeacherProfileAsync(int id)
        {
            var profile = await _dbSet
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            
            if (profile == null)
            {
                return false;
            }

            profile.IsDeleted = true;
            profile.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TeacherProfileExistsAsync(int id)
        {
            return await _dbSet
                .AnyAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<bool> TeacherProfileExistsByUserIdAsync(int userId)
        {
            return await _dbSet
                .AnyAsync(p => p.UserId == userId && !p.IsDeleted);
        }
    }
}