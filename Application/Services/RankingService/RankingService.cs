using AutoMapper;
using EduShpere.Application.DTOs.SearchDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Application.Services.RankingService
{
    public class RankingService : IRankingService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly EduShpere.Infrastructure.Repositories.IActivityRepository _activityRepository;
        private readonly IMapper _mapper;

        public RankingService(
            IUserRepository userRepository,
            IPostRepository postRepository,
            EduShpere.Infrastructure.Repositories.IActivityRepository activityRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
            _activityRepository = activityRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserSearchResultDto>> RankUsersAsync(IEnumerable<User> users, string query, int userId)
        {
            var userScores = new List<(User user, double score)>();

            foreach (var user in users)
            {
                var score = CalculateUserRelevanceScore(user, query, userId);
                userScores.Add((user, score));
            }

            // Sort by score descending
            var rankedUsers = userScores
                .OrderByDescending(x => x.score)
                .Select(x => x.user)
                .ToList();

            // Map to DTOs
            var result = new List<UserSearchResultDto>();
            foreach (var user in rankedUsers)
            {
                var userDto = new UserSearchResultDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    Email = user.Email ?? "",
                    Role = user.Role.HasValue ? (int)user.Role.Value : 0,
                    AvatarUrl = user.AvatarUrl,
                    CreatedAt = user.CreatedAt ?? DateTime.MinValue,
                    StudentNumber = user.StudentProfile?.StudentNumber,
                    TeacherCode = user.TeacherProfile?.TeacherCode,
                    ClassName = user.ClassGroupMembers?.FirstOrDefault()?.ClassGroup?.Name,
                    Department = user.TeacherProfile?.Department
                };
                result.Add(userDto);
            }

            return result;
        }

        public async Task<IEnumerable<PostSearchResultDto>> RankPostsAsync(IEnumerable<Post> posts, string query, int userId)
        {
            var postScores = new List<(Post post, double score)>();

            foreach (var post in posts)
            {
                var score = CalculatePostRelevanceScore(post, query, userId);
                postScores.Add((post, score));
            }

            // Sort by score descending
            var rankedPosts = postScores
                .OrderByDescending(x => x.score)
                .Select(x => x.post)
                .ToList();

            // Map to DTOs with engagement stats
            var result = new List<PostSearchResultDto>();
            foreach (var post in rankedPosts)
            {
                var postDto = _mapper.Map<PostSearchResultDto>(post);
                
                // Add engagement stats
                var stats = await _postRepository.GetPostEngagementStatsAsync(new[] { post.Id });
                if (stats.ContainsKey(post.Id))
                {
                    postDto.LikesCount = stats[post.Id].LikesCount;
                    postDto.CommentsCount = stats[post.Id].CommentsCount;
                }

                // Add hashtags
                postDto.Hashtags = post.PostHashtags?.Select(ph => ph.Hashtag.Name) ?? new List<string>();

                // Generate highlight snippet
                postDto.Highlight = GenerateHighlight(post.Body, query);

                result.Add(postDto);
            }

            return result;
        }

        public async Task<IEnumerable<ActivitySearchResultDto>> RankActivitiesAsync(IEnumerable<Activity> activities, string query, int userId)
        {
            var activityScores = new List<(Activity activity, double score)>();

            foreach (var activity in activities)
            {
                var score = CalculateActivityRelevanceScore(activity, query, userId);
                activityScores.Add((activity, score));
            }

            // Sort by score descending
            var rankedActivities = activityScores
                .OrderByDescending(x => x.score)
                .Select(x => x.activity)
                .ToList();

            // Map to DTOs
            var result = new List<ActivitySearchResultDto>();
            foreach (var activity in rankedActivities)
            {
                var activityDto = _mapper.Map<ActivitySearchResultDto>(activity);
                
                // Add participants count
                activityDto.ParticipantsCount = activity.ActivityParticipants?.Count(ap => !ap.IsDeleted) ?? 0;
                
                result.Add(activityDto);
            }

            return result;
        }

        public double CalculateRelevanceScore<T>(T item, string query, int userId) where T : class
        {
            return item switch
            {
                User user => CalculateUserRelevanceScore(user, query, userId),
                Post post => CalculatePostRelevanceScore(post, query, userId),
                Activity activity => CalculateActivityRelevanceScore(activity, query, userId),
                _ => 0.0
            };
        }

        /// <summary>
        /// Check if two strings match ignoring accents (Vietnamese characters)
        /// </summary>
        private bool IsAccentInsensitiveMatch(string text, string query)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query))
                return false;

            // Remove accents from both strings for comparison
            var normalizedText = RemoveAccents(text);
            var normalizedQuery = RemoveAccents(query);

            return normalizedText.Contains(normalizedQuery);
        }

        /// <summary>
        /// Remove Vietnamese accents from text
        /// </summary>
        private string RemoveAccents(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Vietnamese accent mapping
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

        private double CalculateUserRelevanceScore(User user, string query, int userId)
        {
            double score = 0;
            var queryLower = query.ToLower();
            var queryWords = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            Console.WriteLine($"Calculating score for user: {user.FirstName} {user.LastName} (ID: {user.Id})");

            // 1. FirstName scoring (highest priority) - with accent-insensitive matching
            if (!string.IsNullOrEmpty(user.FirstName))
            {
                var firstName = user.FirstName.ToLower();
                var firstNameScore = 0;
                
                if (firstName == queryLower)
                    firstNameScore = 30; // Exact match
                else if (firstName.StartsWith(queryLower))
                    firstNameScore = 25; // Starts with
                else if (firstName.Contains(queryLower))
                    firstNameScore = 20; // Contains
                else if (IsAccentInsensitiveMatch(firstName, queryLower))
                    firstNameScore = 22; // Accent-insensitive match
                    
                score += firstNameScore;
                Console.WriteLine($"FirstName '{user.FirstName}' score: {firstNameScore}");
            }

            // 2. LastName scoring (highest priority) - with accent-insensitive matching
            if (!string.IsNullOrEmpty(user.LastName))
            {
                var lastName = user.LastName.ToLower();
                var lastNameScore = 0;
                
                if (lastName == queryLower)
                    lastNameScore = 30; // Exact match
                else if (lastName.StartsWith(queryLower))
                    lastNameScore = 25; // Starts with
                else if (lastName.Contains(queryLower))
                    lastNameScore = 20; // Contains
                else if (IsAccentInsensitiveMatch(lastName, queryLower))
                    lastNameScore = 22; // Accent-insensitive match
                    
                score += lastNameScore;
                Console.WriteLine($"LastName '{user.LastName}' score: {lastNameScore}");
            }

            // 3. Email scoring (high priority)
            if (!string.IsNullOrEmpty(user.Email))
            {
                var email = user.Email.ToLower();
                var emailScore = 0;
                
                if (email == queryLower)
                    emailScore = 25; // Exact match
                else if (email.StartsWith(queryLower))
                    emailScore = 20; // Starts with
                else if (email.Contains(queryLower))
                    emailScore = 15; // Contains
                    
                score += emailScore;
                Console.WriteLine($"Email '{user.Email}' score: {emailScore}");
            }

            // 4. Full name exact match (highest priority) - with accent-insensitive matching
            if (!string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName))
            {
                var fullName = (user.FirstName + " " + user.LastName).ToLower();
                var reverseFullName = (user.LastName + " " + user.FirstName).ToLower();
                var fullNameScore = 0;
                
                if (fullName == queryLower || reverseFullName == queryLower)
                    fullNameScore = 35; // Exact full name match
                else if (fullName.Contains(queryLower) || reverseFullName.Contains(queryLower))
                    fullNameScore = 25; // Full name contains
                else if (IsAccentInsensitiveMatch(fullName, queryLower) || IsAccentInsensitiveMatch(reverseFullName, queryLower))
                    fullNameScore = 28; // Accent-insensitive full name match
                    
                score += fullNameScore;
                Console.WriteLine($"Full name '{user.FirstName} {user.LastName}' score: {fullNameScore}");
            }

            // 5. Multi-word search bonus - with accent-insensitive matching
            if (queryWords.Length > 1 && !string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName))
            {
                var fullName = (user.FirstName + " " + user.LastName).ToLower();
                var reverseFullName = (user.LastName + " " + user.FirstName).ToLower();
                var multiWordScore = 0;
                
                // Check if all words are found in the full name
                if (queryWords.All(word => fullName.Contains(word)) || 
                    queryWords.All(word => reverseFullName.Contains(word)))
                    multiWordScore = 20;
                
                // Check if words are found in first name or last name
                else if (queryWords.All(word => 
                    user.FirstName.ToLower().Contains(word) || 
                    user.LastName.ToLower().Contains(word)))
                    multiWordScore = 15;
                
                // Accent-insensitive multi-word matching
                else if (queryWords.All(word => 
                    IsAccentInsensitiveMatch(user.FirstName.ToLower(), word) || 
                    IsAccentInsensitiveMatch(user.LastName.ToLower(), word)))
                    multiWordScore = 18;
                    
                score += multiWordScore;
                Console.WriteLine($"Multi-word search score: {multiWordScore}");
            }

            // 6. Username scoring (medium priority)
            if (!string.IsNullOrEmpty(user.Username))
            {
                var username = user.Username.ToLower();
                var usernameScore = 0;
                
                if (username == queryLower)
                    usernameScore = 15;
                else if (username.StartsWith(queryLower))
                    usernameScore = 12;
                else if (username.Contains(queryLower))
                    usernameScore = 8;
                    
                score += usernameScore;
                Console.WriteLine($"Username '{user.Username}' score: {usernameScore}");
            }

            // 7. Student/Teacher code matching (lower priority)
            if (!string.IsNullOrEmpty(user.StudentProfile?.StudentNumber) && 
                user.StudentProfile.StudentNumber.ToLower().Contains(queryLower))
                score += 10;
            
            if (!string.IsNullOrEmpty(user.TeacherProfile?.TeacherCode) && 
                user.TeacherProfile.TeacherCode.ToLower().Contains(queryLower))
                score += 10;

            // 8. Department matching (lower priority)
            if (!string.IsNullOrEmpty(user.TeacherProfile?.Department) && 
                user.TeacherProfile.Department.ToLower().Contains(queryLower))
                score += 8;

            // 9. Class name matching (lower priority)
            var className = user.ClassGroupMembers?.FirstOrDefault()?.ClassGroup?.Name;
            if (!string.IsNullOrEmpty(className) && className.ToLower().Contains(queryLower))
                score += 6;

            // 10. Recency boost (newer users get slight boost)
            if (user.CreatedAt.HasValue)
            {
                var daysSinceCreated = (DateTime.Now - user.CreatedAt.Value).TotalDays;
                score += Math.Max(0, 2 - daysSinceCreated * 0.01);
            }

            Console.WriteLine($"Total score for user {user.FirstName} {user.LastName}: {score}");
            return score;
        }

        private double CalculatePostRelevanceScore(Post post, string query, int userId)
        {
            double score = 0;
            var queryLower = query.ToLower();

            // Title matching (highest weight)
            if (!string.IsNullOrEmpty(post.Title) && post.Title.ToLower().Contains(queryLower))
                score += 15;

            // Content matching
            if (!string.IsNullOrEmpty(post.Body) && post.Body.ToLower().Contains(queryLower))
                score += 8;

            // Hashtag matching
            var hashtags = post.PostHashtags?.Select(ph => ph.Hashtag.Name.ToLower()) ?? new List<string>();
            foreach (var hashtag in hashtags)
            {
                if (hashtag.Contains(queryLower))
                    score += 6;
            }

            // Exact match bonus
            if (post.Title?.ToLower() == queryLower)
                score += 10;

            // Starts with bonus
            if (post.Title?.ToLower().StartsWith(queryLower) == true)
                score += 5;

            // Engagement boost
            var likesCount = post.PostLikes?.Count(pl => !pl.IsDeleted) ?? 0;
            var commentsCount = post.Comments?.Count(c => !c.IsDeleted) ?? 0;
            
            score += Math.Log(likesCount + 1) * 2;
            score += Math.Log(commentsCount + 1) * 1.5;

            // Recency boost
            if (post.CreatedAt.HasValue)
            {
                var daysSinceCreated = (DateTime.Now - post.CreatedAt.Value).TotalDays;
                score += Math.Max(0, 5 - daysSinceCreated * 0.1);
            }

            // Author relationship boost
            if (post.UserId == userId)
                score += 3;

                // Status boost
                if (post.Status == Domain.Enum.PostStatus.Published)
                    score += 2;

                // Privacy boost (public posts get slight boost)
                if (post.PrivacyLevel == (int)Domain.Enum.PostVisibility.Public)
                    score += 1;

            return score;
        }

        private double CalculateActivityRelevanceScore(Activity activity, string query, int userId)
        {
            double score = 0;
            var queryLower = query.ToLower();

            // Title matching (highest weight)
            if (!string.IsNullOrEmpty(activity.Title) && activity.Title.ToLower().Contains(queryLower))
                score += 15;

            // Description matching
            if (!string.IsNullOrEmpty(activity.Description) && activity.Description.ToLower().Contains(queryLower))
                score += 8;

            // Location matching
            if (!string.IsNullOrEmpty(activity.Location) && activity.Location.ToLower().Contains(queryLower))
                score += 6;

            // Organizer matching
            if (!string.IsNullOrEmpty(activity.Organizer) && activity.Organizer.ToLower().Contains(queryLower))
                score += 5;

            // Exact match bonus
            if (activity.Title?.ToLower() == queryLower)
                score += 10;

            // Starts with bonus
            if (activity.Title?.ToLower().StartsWith(queryLower) == true)
                score += 5;

            // Participants boost
            var participantsCount = activity.ActivityParticipants?.Count(ap => !ap.IsDeleted) ?? 0;
            score += Math.Log(participantsCount + 1) * 2;

            // Recency boost (upcoming activities get higher score)
            if (activity.StartDate.HasValue)
            {
                var daysUntilStart = (activity.StartDate.Value - DateTime.Now).TotalDays;
                if (daysUntilStart > 0) // Future activity
                    score += Math.Max(0, 5 - daysUntilStart * 0.05);
                else // Past activity
                    score += Math.Max(0, 2 - Math.Abs(daysUntilStart) * 0.02);
            }

                // Category boost
                score += 1;

            return score;
        }

        private string GenerateHighlight(string? content, string query, int maxLength = 150)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(query))
                return content?.Substring(0, Math.Min(maxLength, content.Length)) ?? "";

            var queryLower = query.ToLower();
            var contentLower = content.ToLower();
            var index = contentLower.IndexOf(queryLower);

            if (index == -1)
                return content.Substring(0, Math.Min(maxLength, content.Length));

            // Find optimal snippet around the match
            var start = Math.Max(0, index - maxLength / 3);
            var end = Math.Min(content.Length, start + maxLength);
            
            var snippet = content.Substring(start, end - start);
            
            // Add ellipsis if needed
            if (start > 0) snippet = "..." + snippet;
            if (end < content.Length) snippet = snippet + "...";

            return snippet;
        }
    }
}
