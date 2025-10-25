

using System;
using EduShpere.Domain;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(EduShpereDbContext context) : base(context) {
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Entity not found");

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
        public virtual async Task<User?> GetByIdIncludeAsync(int id)
        {
            return await _dbSet.Include(p=>p.UserRights).ThenInclude(p=>p.Right).FirstOrDefaultAsync(p=>p.Id==id);
        }
        public IQueryable<User> GetAllByStaffIncluding()
        {
            return  _dbSet.Include(p => p.UserRights).ThenInclude(p => p.Right).Where(p => p.Role == UserRole.Staff);
        }
        public async Task<User> GetByStaffIdIncluding(int staffId)
        {
            return await _dbSet.Include(p => p.UserRights).ThenInclude(p => p.Right).Where(p => p.Role == UserRole.Staff && p.Id==staffId).FirstOrDefaultAsync();
        }

        public virtual async Task<User?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<User?> GetUserByUserName(string userName)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Username == userName || p.Email.Equals(userName));
        }

        public async Task<bool> FindUserByEmail(string email)
        {
            return await _dbSet.AnyAsync(p => p.Email == email);
        }
        public async Task<User?> GetUserByEmail(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<bool> FindUserByUsername(string username)
        {
            return await _dbSet.AnyAsync(p => p.Username == username);
        }

        public IQueryable<User> GetQueryable()
        {
            return _dbSet
                .Where(u => !u.IsDeleted)
                .Include(u => u.StudentProfile)
                .Include(u => u.TeacherProfile)
                .Include(u => u.ClassGroupMembers)
                    .ThenInclude(cgm => cgm.ClassGroup);
        }

        public async Task<IEnumerable<User>> SearchAsync(string query, int limit = 10)
        {
            try
            {
                var trimmedQuery = query.Trim().ToLower();
                var normalizedQuery = RemoveVietnameseAccents(trimmedQuery);

                // Split query into words for better matching
                var queryWords = normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                Console.WriteLine($"Searching users with query: '{trimmedQuery}' -> normalized: '{normalizedQuery}', words: [{string.Join(", ", queryWords)}]");

                // Get all users for client-side search
                var allUsersForSearch = await _dbSet
                    .Where(u => !u.IsDeleted)
                    .Include(u => u.StudentProfile)
                    .Include(u => u.TeacherProfile)
                    .Include(u => u.ClassGroupMembers)
                        .ThenInclude(cgm => cgm.ClassGroup)
                    .ToListAsync();

                // Apply client-side filtering with accent-insensitive matching using the provided logic
                var users = allUsersForSearch.AsEnumerable()
                    .Where(u => {
                        // Combine all searchable fields
                        var searchableTexts = new List<string>();

                        if (u.FirstName != null) {
                            searchableTexts.Add(RemoveVietnameseAccents(u.FirstName.Trim().ToLower()));
                        }

                        if (u.LastName != null) {
                            searchableTexts.Add(RemoveVietnameseAccents(u.LastName.Trim().ToLower()));
                        }

                        if (u.StudentProfile?.StudentNumber != null) {
                            searchableTexts.Add(RemoveVietnameseAccents(u.StudentProfile.StudentNumber.ToLower()));
                        }

                        if (u.TeacherProfile?.TeacherCode != null) {
                            searchableTexts.Add(RemoveVietnameseAccents(u.TeacherProfile.TeacherCode.ToLower()));
                        }

                        // Check if all query words are found in any of the searchable texts
                        return queryWords.All(queryWord =>
                            searchableTexts.Any(text => text.Contains(queryWord))
                        );
                    })
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Take(limit)
                .ToList();

                Console.WriteLine($"Search for '{query}' found {users.Count} users");

                // Log some sample results for debugging
                if (users.Any())
                {
                    var sampleResults = users.Take(3).Select(u =>
                        $"ID:{u.Id}, FirstName:'{u.FirstName}', LastName:'{u.LastName}', Email:'{u.Email}'"
                    );
                    Console.WriteLine($"Sample results: {string.Join("; ", sampleResults)}");
                }

                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Fallback: Return empty list instead of all users
                return new List<User>();
            }
        }

        /// <summary>
        /// Remove Vietnamese accents from text
        /// </summary>
        private string RemoveVietnameseAccents(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var accentMap = new Dictionary<char, char>
            {
                {'à', 'a'}, {'á', 'a'}, {'ạ', 'a'}, {'ả', 'a'}, {'ã', 'a'}, {'â', 'a'}, {'ầ', 'a'}, {'ấ', 'a'}, {'ậ', 'a'}, {'ẩ', 'a'}, {'ẫ', 'a'}, {'ă', 'a'}, {'ằ', 'a'}, {'ắ', 'a'}, {'ặ', 'a'}, {'ẳ', 'a'}, {'ẵ', 'a'},
                {'è', 'e'}, {'é', 'e'}, {'ẹ', 'e'}, {'ẻ', 'e'}, {'ẽ', 'e'}, {'ê', 'e'}, {'ề', 'e'}, {'ế', 'e'}, {'ệ', 'e'}, {'ể', 'e'}, {'ễ', 'e'},
                {'ì', 'i'}, {'í', 'i'}, {'ị', 'i'}, {'ỉ', 'i'}, {'ĩ', 'i'},
                {'ò', 'o'}, {'ó', 'o'}, {'ọ', 'o'}, {'ỏ', 'o'}, {'õ', 'o'}, {'ô', 'o'}, {'ồ', 'o'}, {'ố', 'o'}, {'ộ', 'o'}, {'ổ', 'o'}, {'ỗ', 'o'}, {'ơ', 'o'}, {'ờ', 'o'}, {'ớ', 'o'}, {'ợ', 'o'}, {'ở', 'o'}, {'ỡ', 'o'},
                {'ù', 'u'}, {'ú', 'u'}, {'ụ', 'u'}, {'ủ', 'u'}, {'ũ', 'u'}, {'ư', 'u'}, {'ừ', 'u'}, {'ứ', 'u'}, {'ự', 'u'}, {'ử', 'u'}, {'ữ', 'u'},
                {'ỳ', 'y'}, {'ý', 'y'}, {'ỵ', 'y'}, {'ỷ', 'y'}, {'ỹ', 'y'},
                {'đ', 'd'},
                {'À', 'A'}, {'Á', 'A'}, {'Ạ', 'A'}, {'Ả', 'A'}, {'Ã', 'A'}, {'Â', 'A'}, {'Ầ', 'A'}, {'Ấ', 'A'}, {'Ậ', 'A'}, {'Ẩ', 'A'}, {'Ẫ', 'A'}, {'Ă', 'A'}, {'Ằ', 'A'}, {'Ắ', 'A'}, {'Ặ', 'A'}, {'Ẳ', 'A'}, {'Ẵ', 'A'},
                {'È', 'E'}, {'É', 'E'}, {'Ẹ', 'E'}, {'Ẻ', 'E'}, {'Ẽ', 'E'}, {'Ê', 'E'}, {'Ề', 'E'}, {'Ế', 'E'}, {'Ệ', 'E'}, {'Ể', 'E'}, {'Ễ', 'E'},
                {'Ì', 'I'}, {'Í', 'I'}, {'Ị', 'I'}, {'Ỉ', 'I'}, {'Ĩ', 'I'},
                {'Ò', 'O'}, {'Ó', 'O'}, {'Ọ', 'O'}, {'Ỏ', 'O'}, {'Õ', 'O'}, {'Ô', 'O'}, {'Ồ', 'O'}, {'Ố', 'O'}, {'Ộ', 'O'}, {'Ổ', 'O'}, {'Ỗ', 'O'}, {'Ơ', 'O'}, {'Ờ', 'O'}, {'Ớ', 'O'}, {'Ợ', 'O'}, {'Ở', 'O'}, {'Ỡ', 'O'},
                {'Ù', 'U'}, {'Ú', 'U'}, {'Ụ', 'U'}, {'Ủ', 'U'}, {'Ũ', 'U'}, {'Ư', 'U'}, {'Ừ', 'U'}, {'Ứ', 'U'}, {'Ự', 'U'}, {'Ử', 'U'}, {'Ữ', 'U'},
                {'Ỳ', 'Y'}, {'Ý', 'Y'}, {'Ỵ', 'Y'}, {'Ỷ', 'Y'}, {'Ỹ', 'Y'},
                {'Đ', 'D'}
            };

            var result = new System.Text.StringBuilder();
            foreach (char c in text)
            {
                if (accentMap.ContainsKey(c))
                    result.Append(accentMap[c]);
                else
                    result.Append(c);
            }
            return result.ToString();
        }
            public async Task UpdateRangeAsync(IEnumerable<User> users)
            {
                _dbSet.UpdateRange(users);
                await _context.SaveChangesAsync();
            }

        }
    } 
