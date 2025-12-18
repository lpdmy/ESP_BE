using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.AuthDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.UserDto;
using EduShpere.Application.Services;
using EduShpere.Domain;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Infrastructure.Repositories.OneTimeLogin;
using EduShpere.Infrastructure.Repositories.TeacherProfile;
using EduShpere.Infrastructure.Security;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TeacherProfileEntity = EduShpere.Domain.Models.TeacherProfile;
namespace EduShpere.Application
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextService _httpContextService;
        private readonly IOneTimeLoginRepository _oneTimeLoginRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IStudentProfileRepository _studentProfileRepository;
        private readonly ITeacherProfileRepository _teacherProfileRepository;
        private readonly IAuditService _auditService;
        private readonly IPaginationService _paginationService;
        private readonly IUserRightRepository _userRightRepository;

        public AuthService(IUserRepository userRepository, IConfiguration configuration, IHttpContextService httpContextService, IMapper mapper, IOneTimeLoginRepository oneTimeLoginRepository, IEmailService emailService, IStudentProfileRepository studentProfileRepository, ITeacherProfileRepository teacherProfileRepository, IAuditService auditService, IPaginationService paginationService, IUserRightRepository userRightRepository)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _httpContextService = httpContextService;
            _mapper = mapper;
            _oneTimeLoginRepository = oneTimeLoginRepository;
            _emailService = emailService;
            _studentProfileRepository = studentProfileRepository;
            _teacherProfileRepository = teacherProfileRepository;
            _auditService = auditService;
            _paginationService = paginationService;
            _userRightRepository = userRightRepository;
        }

        public async Task<UserDto> GetMe()
        {
            // Lấy userId trực tiếp từ JWT claims để tránh 1 query dư thừa
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
            {
                throw new UnauthorizedException(ErrorMessages.Auth.Unauthorized);
            }

            // Dùng phương thức tối ưu riêng cho GetMe (AsNoTracking + include cần thiết)
            var fullUser = await _userRepository.GetUserForMeAsync(userId.Value);
            if (fullUser == null)
            {
                throw new NotFoundException(ErrorMessages.Auth.UserNotFound);
            }

            return _mapper.Map<UserDto>(fullUser);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// Tạo JWT access token + refresh token cho user.
        /// Đã tối ưu để chỉ load quyền (UserRights) thay vì include toàn bộ navigation nặng.
        /// </summary>
        private async Task<TokenModel> GenerateTokenAsync(User appUser)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["AppSetting:SecretKey"];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            
            // Chỉ load user + quyền, tránh include StudentProfile, TeacherProfile, ClassGroup... nặng
            var user = await _userRepository.GetByIdIncludeAsync(appUser.Id);
            if (user == null)
            {
                throw new NotFoundException(ErrorMessages.Auth.UserNotFound);
            }
            // Convert role number to role name
            var roleName = appUser.Role switch
            {
                UserRole.Admin => "Admin",
                UserRole.Teacher => "Teacher",
                UserRole.Student => "Student",
                UserRole.Staff=>"Staff",
                _ => "Student"
            };
            var permissions = user.UserRights
            .Where(ur => !ur.IsDeleted)
            .Select(ur => ur.Right.Code)
            .ToList();
            var claims = new List<Claim>
            {
               new Claim("Email", appUser.Email),
               new Claim("FullName", appUser.FirstName),
               new Claim("Id", appUser.Id.ToString()),
               new Claim("UserName", appUser.Username?? string.Empty),
               new Claim("UserRole", appUser.Role.ToString()),
               new Claim(ClaimTypes.Role, roleName)
            };
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var accessTokenString = jwtTokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            return new TokenModel
            {
                AccessToken = accessTokenString,
                RefreshToken = refreshToken
            };
        }

        public async Task<TokenModel> Login(LoginDto dto)
        {
            // Validate email và password - cấm dấu cách và chữ tiếng Việt
            if (ContainsVietnameseOrSpaces(dto.Username))
            {
                throw new BadRequestException(ErrorMessages.Auth.EmailContainsSpacesOrVietnamese);
            }

            if (ContainsVietnameseOrSpaces(dto.Password))
            {
                throw new BadRequestException(ErrorMessages.Auth.PasswordContainsSpacesOrVietnamese);
            }

            var user = await _userRepository.GetUserByUserName(dto.Username);

            if (user == null)
            {
                throw new UnauthorizedException(ErrorMessages.Auth.InvalidCredentials);
            }

            // Verify password bằng hash
            var isPasswordValid = PasswordHelper.VerifyPassword(user, user.Password, dto.Password);
            if (!isPasswordValid)
            {
                throw new UnauthorizedException(ErrorMessages.Auth.InvalidCredentials);
            }

            // Tạo token async, chỉ query đúng phần cần (user + quyền)
            return await GenerateTokenAsync(user);
        }

        public async Task<object> ImportUsers(IFormFile request)
        {
            if (request == null || request.Length == 0)
                throw new BadRequestException(ErrorMessages.Auth.InvalidFile);

            var users = new List<User>();

            using var stream = new MemoryStream();
            await request.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();
            var rowCount = worksheet.LastRowUsed().RowNumber();

            for (int row = 2; row <= rowCount; row++)
            {
                string fullName = worksheet.Cell(row, 1).GetValue<string>().Trim();
                if (string.IsNullOrWhiteSpace(fullName) || worksheet.Cell(row, 2).IsEmpty())
                    continue;

                DateTime dob;
                try
                {
                    dob = worksheet.Cell(row, 2).GetDateTime();
                }
                catch
                {
                    string dobText = worksheet.Cell(row, 2).GetValue<string>().Trim();
                    if (!DateTime.TryParseExact(dobText,
                        new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" },
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out dob))
                        continue;
                }

                string username = GenerateUsername(fullName, dob);

                if (await _userRepository.FindUserByEmail(username))
                    continue;

                string email = username;
                string password = "123";

                users.Add(new User
                {
                    FirstName = fullName,
                    Birthdate = dob,
                    Email = email,
                    Password = password
                });
            }

            if (users.Any())
                await _userRepository.AddRangeAsync(users);

            return new
            {
                CreatedCount = users.Count,
                Message = $"Đã thêm {users.Count} user mới."
            };
        }

        private string GenerateUsername(string fullName, DateTime dob)
        {
            string namePart = RemoveVietnameseSigns(fullName).Replace(" ", "").ToLower();
            string year = DateTime.Now.Year.ToString().Substring(2, 2);
            string dayMonth = dob.ToString("ddMM");
            return $"{namePart}{year}{dayMonth}";
        }

        private string RemoveVietnameseSigns(string text)
        {
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                {
                    text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
                }
            }

            return text;
        }

        private bool ContainsVietnameseOrSpaces(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Kiểm tra dấu cách
            if (input.Contains(" "))
                return true;

            // Kiểm tra ký tự tiếng Việt
            string[] vietnameseSigns = new string[]
            {
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            foreach (string vietnameseSign in vietnameseSigns)
            {
                foreach (char c in vietnameseSign)
                {
                    if (input.Contains(c))
                        return true;
                }
            }

            return false;
        }

        private void ValidateStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new BadRequestException(ErrorMessages.Password.PasswordInvalidLength);

            // Kiểm tra độ dài (6-20 ký tự)
            if (password.Length < 6)
                throw new BadRequestException(ErrorMessages.Password.PasswordTooShort);

            if (password.Length > 20)
                throw new BadRequestException(ErrorMessages.Password.PasswordTooLong);

            // Kiểm tra chữ cái viết hoa
            if (!password.Any(char.IsUpper))
                throw new BadRequestException(ErrorMessages.Password.PasswordMustContainUppercase);

            // Kiểm tra ký tự đặc biệt
            string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            if (!password.Any(c => specialChars.Contains(c)))
                throw new BadRequestException(ErrorMessages.Password.PasswordMustContainSpecialChar);
        }

        public async Task<bool> CreateUserAndGenerateOtlAsync(CreateUserDto dto)
        {
            // Validate email - cấm dấu cách và chữ tiếng Việt
            if (ContainsVietnameseOrSpaces(dto.Email))
            {
                throw new BadRequestException(ErrorMessages.Auth.EmailContainsSpacesOrVietnamese);
            }

            if (await _userRepository.FindUserByEmail(dto.Email))
            {
                throw new BadRequestException(ErrorMessages.Auth.EmailAlreadyExists);
            }

            // Validate username - cấm dấu cách và chữ tiếng Việt
            if (ContainsVietnameseOrSpaces(dto.Username))
            {
                throw new BadRequestException(ErrorMessages.Auth.UsernameContainsSpacesOrVietnamese);
            }

            if (await _userRepository.FindUserByUsername(dto.Username))
            {
                throw new BadRequestException(ErrorMessages.Auth.UsernameAlreadyExists);
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Role = dto.Role,
                Password = PasswordHelper.HashPassword(new User(), "123"),
                Username = dto.Username, // Username giữ nguyên (mã số hiển thị)
                PhoneNumber = dto.PhoneNumber,
                Birthdate = dto.BirthDate,
                AvatarUrl = dto.AvatarUrl
            };

            _auditService.SetAuditFieldsForCreate(user);
            await _userRepository.AddAsync(user);

            // Tự động tạo profile nếu là Student
            if (dto.Role == UserRole.Student)
            {
                var profile = new StudentProfile
                {
                    UserId = user.Id,
                    StudentNumber = dto.StudentNumber ?? dto.Username, // Sử dụng Username làm StudentNumber mặc định
                    EnrollmentYear = dto.EnrollmentYear ?? (short)DateTime.Now.Year,
                    Bio = dto.Bio,
                    ExtraJson = dto.ExtraJson
                };

                // Sử dụng CreateStudentProfileAsync để có thể truyền classGroupId
                await _studentProfileRepository.CreateStudentProfileAsync(profile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl, dto.ClassGroupId);
            }
            // Tự động tạo teacher profile nếu là Teacher
            else if (dto.Role == UserRole.Teacher)
            {
                var teacherProfile = new TeacherProfileEntity
                {
                    UserId = user.Id,
                    TeacherCode = dto.TeacherCode ?? dto.Username, // Sử dụng Username làm TeacherCode mặc định
                    Department = dto.Department,
                    Position = dto.Position,
                    Bio = dto.Bio,
                    ExtraJson = dto.ExtraJson
                };

                await _teacherProfileRepository.CreateTeacherProfileAsync(teacherProfile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl);
            }

            var otlToken = await _oneTimeLoginRepository.CreateTokenAsync(user);
            var baseUrl = _configuration["AppSetting:FrontEndUrl"];
            var loginLink = $"{baseUrl}/auth/one-time-login?token={otlToken.Token}";
            await _emailService.SendEmailAsync(dto.Email, "Login Now", EmailTemplate.GetInvitationEmail(user.LastName, loginLink), true);

            return true;
        }

        public async Task<TokenModel> CreateUserAndReturnTokenAsync(CreateUserDto dto)
        {
            // Validate email - cấm dấu cách và chữ tiếng Việt
            if (ContainsVietnameseOrSpaces(dto.Email))
            {
                throw new BadRequestException(ErrorMessages.Auth.EmailContainsSpacesOrVietnamese);
            }

            if (await _userRepository.FindUserByEmail(dto.Email))
            {
                throw new BadRequestException(ErrorMessages.Auth.EmailAlreadyExists);
            }

            // Validate username - cấm dấu cách và chữ tiếng Việt
            if (ContainsVietnameseOrSpaces(dto.Username))
            {
                throw new BadRequestException(ErrorMessages.Auth.UsernameContainsSpacesOrVietnamese);
            }

            if (await _userRepository.FindUserByUsername(dto.Username))
            {
                throw new BadRequestException(ErrorMessages.Auth.UsernameAlreadyExists);
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Role = dto.Role,
                Password = PasswordHelper.HashPassword(new User(), "123"),
                Username = dto.Username,
                PhoneNumber = dto.PhoneNumber,
                Birthdate = dto.BirthDate,
                AvatarUrl = dto.AvatarUrl
            };

            _auditService.SetAuditFieldsForCreate(user);
            await _userRepository.AddAsync(user);

            // Tự động tạo profile nếu là Student
            if (dto.Role == UserRole.Student)
            {
                var profile = new StudentProfile
                {
                    UserId = user.Id,
                    StudentNumber = dto.StudentNumber ?? dto.Username,
                    EnrollmentYear = dto.EnrollmentYear ?? (short)DateTime.Now.Year,
                    Bio = dto.Bio,
                    ExtraJson = dto.ExtraJson
                };

                // Sử dụng CreateStudentProfileAsync để có thể truyền classGroupId
                await _studentProfileRepository.CreateStudentProfileAsync(profile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl, dto.ClassGroupId);
            }
            // Tự động tạo teacher profile nếu là Teacher
            else if (dto.Role == UserRole.Teacher)
            {
                var teacherProfile = new TeacherProfileEntity
                {
                    UserId = user.Id,
                    TeacherCode = dto.TeacherCode ?? dto.Username, // Sử dụng Username làm TeacherCode mặc định
                    Department = dto.Department,
                    Position = dto.Position,
                    Bio = dto.Bio,
                    ExtraJson = dto.ExtraJson
                };

                await _teacherProfileRepository.CreateTeacherProfileAsync(teacherProfile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl);
            }

            // Trả về token thay vì gửi email (chỉ dành cho development)
            return await GenerateTokenAsync(user);
        }

        public async Task<PaginationResponseDto<UserDto>> GetAllUsersAsync(UserPaginationRequestDto paginationRequest)
        {
            var query = _userRepository.GetQueryable();

            // Add status filter
            if (paginationRequest.Status.HasValue)
            {
                var statusEnum = (UserStatus)paginationRequest.Status.Value;
                query = query.Where(u => u.Status == statusEnum);
            }

            // Add role filter
            if (paginationRequest.Role.HasValue)
            {
                var roleEnum = (UserRole)paginationRequest.Role.Value;
                query = query.Where(u => u.Role == roleEnum);
            }

            // Add search functionality
            if (!string.IsNullOrEmpty(paginationRequest.Search))
            {
                var searchTerm = paginationRequest.Search.ToLower();
                query = query.Where(u =>
                    (u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(searchTerm)) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchTerm)) ||
                    (u.Username != null && u.Username.ToLower().Contains(searchTerm)) ||
                    (u.StudentProfile != null && u.StudentProfile.StudentNumber != null && u.StudentProfile.StudentNumber.ToLower().Contains(searchTerm)) ||
                    (u.TeacherProfile != null && u.TeacherProfile.TeacherCode != null && u.TeacherProfile.TeacherCode.ToLower().Contains(searchTerm))
                );
            }

            // Apply custom sorting for User entity
            if (!string.IsNullOrEmpty(paginationRequest.SortBy))
            {
                query = ApplyUserSorting(query, paginationRequest.SortBy, paginationRequest.SortDescending);
            }

            // Create a copy of paginationRequest without SortBy to avoid double sorting
            var paginationRequestWithoutSort = new PaginationRequestDto
            {
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize,
                Search = paginationRequest.Search
                // SortBy and SortDescending are intentionally omitted
            };

            var pagedResult = await _paginationService.GetPagedResultAsync(query, paginationRequestWithoutSort);

            return new PaginationResponseDto<UserDto>
            {
                Data = _mapper.Map<IEnumerable<UserDto>>(pagedResult.Data),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }

        private IQueryable<User> ApplyUserSorting(IQueryable<User> query, string sortBy, bool sortDescending)
        {
            return sortBy.ToLower() switch
            {
                "id" => sortDescending
                    ? query.OrderByDescending(u => u.StudentProfile != null ? u.StudentProfile.StudentNumber :
                                                  u.TeacherProfile != null ? u.TeacherProfile.TeacherCode :
                                                  u.Email)
                    : query.OrderBy(u => u.StudentProfile != null ? u.StudentProfile.StudentNumber :
                                        u.TeacherProfile != null ? u.TeacherProfile.TeacherCode :
                                        u.Email),
                "name" => sortDescending
                    ? query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName)
                    : query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
                "email" => sortDescending
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email),
                "role" => sortDescending
                    ? query.OrderByDescending(u => u.Role)
                    : query.OrderBy(u => u.Role),
                /*"class" => sortDescending 
                    ? query.OrderByDescending(u => u.StudentProfile != null ? u.StudentProfile.ClassGroupId : 
                                                  u.TeacherProfile != null ? u.TeacherProfile.Position : "")
                    : query.OrderBy(u => u.StudentProfile != null ? u.StudentProfile.ClassGroupId : 
                                        u.TeacherProfile != null ? u.TeacherProfile.Position : ""),*/
                "status" => sortDescending
                    ? query.OrderByDescending(u => u.Status)
                    : query.OrderBy(u => u.Status),
                _ => query.OrderBy(u => u.Id)
            };
        }

        public async Task<bool> UpdateUserAsync(UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.Id);
            if (user == null)
                throw new NotFoundException(ErrorMessages.Auth.UserNotFound);

            // Check email duplication nếu có thay đổi
            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (ContainsVietnameseOrSpaces(dto.Email))
                    throw new BadRequestException(ErrorMessages.Auth.EmailContainsSpacesOrVietnamese);

                var emailExists = await _userRepository.FindUserByEmail(dto.Email);
                if (emailExists)
                    throw new BadRequestException(ErrorMessages.Auth.EmailAlreadyExists);

                user.Email = dto.Email;
            }

            // Check username duplication nếu có thay đổi
            if (!string.Equals(user.Username, dto.Username, StringComparison.OrdinalIgnoreCase))
            {
                if (ContainsVietnameseOrSpaces(dto.Username))
                    throw new BadRequestException(ErrorMessages.Auth.UsernameContainsSpacesOrVietnamese);

                var usernameExists = await _userRepository.FindUserByUsername(dto.Username);
                if (usernameExists)
                    throw new BadRequestException(ErrorMessages.Auth.UsernameAlreadyExists);

                user.Username = dto.Username;
            }

            // Update các field chung trong User
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Birthdate = dto.BirthDate;
            user.AvatarUrl = dto.AvatarUrl;
            user.Role = dto.Role;
            user.Status = dto.Status;

            _auditService.SetAuditFieldsForUpdate(user);
            await _userRepository.UpdateAsync(user);

            // Cập nhật StudentProfile nếu là Student
            if (dto.Role == UserRole.Student)
            {
                var profile = await _studentProfileRepository.GetStudentProfileByUserIdAsync(user.Id);
                if (profile == null)
                {
                    // Nếu chưa có thì tạo mới
                    /*profile = new StudentProfile
                    {
                        UserId = user.Id,
                        StudentNumber = dto.StudentNumber ?? dto.Username,
                        EnrollmentYear = dto.EnrollmentYear ?? (short)DateTime.Now.Year,
                        Bio = dto.Bio,
                        ExtraJson = dto.ExtraJson
                    };

                    await _studentProfileRepository.CreateStudentProfileAsync(
                        profile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl, dto.ClassGroupId);*/
                    throw new BadRequestException(ErrorMessages.UserProfile.ProfileNotFound);
                }
                else
                {
                    // Cập nhật profile hiện có
                    profile.StudentNumber = dto.StudentNumber ?? profile.StudentNumber;
                    profile.EnrollmentYear = dto.EnrollmentYear ?? profile.EnrollmentYear;
                    //profile.Bio = dto.Bio ?? profile.Bio;
                    //profile.ExtraJson = dto.ExtraJson ?? profile.ExtraJson;
                    //profile.ClassGroupId = dto.ClassGroupId ?? profile.ClassGroupId;

                    await _studentProfileRepository.UpdateStudentProfileAsync(
                        profile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl);
                }
            }
            // Cập nhật TeacherProfile nếu là Teacher
            else if (dto.Role == UserRole.Teacher)
            {
                var profile = await _teacherProfileRepository.GetTeacherProfileByUserIdAsync(user.Id);
                if (profile == null)
                {
                    /*profile = new TeacherProfileEntity
                    {
                        UserId = user.Id,
                        TeacherCode = dto.TeacherCode ?? dto.Username,
                        Department = dto.Department,
                        Position = dto.Position,
                        Bio = dto.Bio,
                        ExtraJson = dto.ExtraJson
                    };

                    await _teacherProfileRepository.CreateTeacherProfileAsync(
                        profile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl);*/
                    throw new BadRequestException(ErrorMessages.UserProfile.ProfileNotFound);
                }
                else
                {
                    // Cập nhật profile hiện có
                    profile.TeacherCode = dto.TeacherCode ?? profile.TeacherCode;
                    profile.Department = dto.Department ?? profile.Department;
                    profile.Position = dto.Position ?? profile.Position;
                    //profile.Bio = dto.Bio ?? profile.Bio;
                    //profile.ExtraJson = dto.ExtraJson ?? profile.ExtraJson;

                    await _teacherProfileRepository.UpdateTeacherProfileAsync(
                        profile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl);
                }
            }

            return true;
        }


        public async Task<string> OneTimeLoginAsync(string token)
        {
            var otl = await _oneTimeLoginRepository.GetValidTokenAsync(token);
            if (otl == null)
                throw new UnauthorizedException(ErrorMessages.Auth.InvalidToken);

            return $"{otl.User.FirstName} {otl.User.LastName}";
        }

        public async Task<TokenModel> ChangePasswordWithOtlAsync(string token, string newPassword)
        {
            var otl = await _oneTimeLoginRepository.GetValidTokenAsync(token);
            if (otl == null)
                throw new UnauthorizedException(ErrorMessages.Auth.InvalidToken);

            var user = await _userRepository.GetByIdAsync(otl.UserId);
            if (user == null)
                throw new NotFoundException(ErrorMessages.Auth.UserNotFound);

            // Validate password - cấm dấu cách và chữ tiếng Việt
            if (ContainsVietnameseOrSpaces(newPassword))
            {
                throw new BadRequestException(ErrorMessages.Auth.PasswordContainsSpacesOrVietnamese);
            }

            // Validate strong password
            ValidateStrongPassword(newPassword);

            user.Password = PasswordHelper.HashPassword(user, newPassword);
            _auditService.SetAuditFieldsForUpdate(user);
            await _userRepository.UpdateAsync(user);
            await _oneTimeLoginRepository.MarkAsUsedAsync(otl);

            return await GenerateTokenAsync(user);
        }

        public async Task<bool> ChangePassword(ChangePasswordDto dto)
        {
            var user = await _httpContextService.GetAppUserAndThrow();

            // Validate passwords - cấm dấu cách và chữ tiếng Việt
            if (ContainsVietnameseOrSpaces(dto.OldPassword))
            {
                throw new BadRequestException(ErrorMessages.Auth.OldPasswordContainsSpacesOrVietnamese);
            }

            if (ContainsVietnameseOrSpaces(dto.NewPassword))
            {
                throw new BadRequestException(ErrorMessages.Auth.NewPasswordContainsSpacesOrVietnamese);
            }

            if (ContainsVietnameseOrSpaces(dto.ConfirmPassword))
            {
                throw new BadRequestException(ErrorMessages.Auth.ConfirmPasswordContainsSpacesOrVietnamese);
            }

            var isOldPasswordValid = PasswordHelper.VerifyPassword(user, user.Password, dto.OldPassword);
            if (!isOldPasswordValid)
            {
                throw new BadRequestException(ErrorMessages.Auth.InvalidPassword);
            }

            // Validate strong password
            ValidateStrongPassword(dto.NewPassword);

            if (dto.NewPassword != dto.ConfirmPassword)
            {
                throw new BadRequestException(ErrorMessages.Password.PasswordMismatch);
            }

            user.Password = PasswordHelper.HashPassword(user, dto.NewPassword);
            _auditService.SetAuditFieldsForUpdate(user);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<TokenModel> ForgotPassword(ForgotPasswordDto dto)
        {
            // Validate email - cấm dấu cách và chữ tiếng Việt
            if (ContainsVietnameseOrSpaces(dto.Email))
            {
                throw new BadRequestException(ErrorMessages.Auth.EmailContainsSpacesOrVietnamese);
            }

            var user = await _userRepository.GetUserByEmail(dto.Email);
            if (user == null)
            {
                throw new NotFoundException(ErrorMessages.Auth.UserNotFound);
            }

            var otlToken = await _oneTimeLoginRepository.CreateTokenAsync(user);

            var baseUrl = _configuration["AppSetting:FrontEndUrl"];
            var resetLink = $"{baseUrl}/auth/one-time-login?token={otlToken.Token}";

            // 4. Gửi email
            await _emailService.SendEmailAsync(
                user.Email,
                "EduSphere - Đặt lại mật khẩu",
                EmailTemplate.GetResetPasswordEmail(user.LastName, resetLink),
                true
            );

            return await GenerateTokenAsync(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException(ErrorMessages.Auth.UserNotFound);

            _auditService.SetAuditFieldsForDelete(user);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            var allUsers = await _userRepository.GetQueryable().ToListAsync();

            var statistics = new UserStatisticsDto
            {
                TotalUsers = allUsers.Count,
                ActiveUsers = allUsers.Count(u => u.Status == UserStatus.Active),
                InactiveUsers = allUsers.Count(u => u.Status == UserStatus.Inactive),
                Students = allUsers.Count(u => u.Role == UserRole.Student),
                Teachers = allUsers.Count(u => u.Role == UserRole.Teacher),
                Admins = allUsers.Count(u => u.Role == UserRole.Admin)
            };

            return statistics;
        }
        public async Task<UserResponseDto> CreateStaff(CreateStaffDto dto)
        {
            var user = await _userRepository.FindUserByEmail(dto.Email);
            if (user)
            {
                throw new BadRequestException(ErrorMessages.Auth.ExistByEmail);
            }
            var staff = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Role = UserRole.Staff,
                Password = PasswordHelper.HashPassword(new User(), dto.Password),
                PhoneNumber = dto.PhoneNumber,
            };
            await _userRepository.AddAsync(staff);
            var userRights = dto.Permission.Select(rid => new UserRight
            {
                UserId = staff.Id,
                RightId = rid,
                IsDeleted = false
            }).ToList();
            await _userRightRepository.AddRangeAsync(userRights);

            var CreatedUster = await _userRepository.GetByIdIncludeAsync(staff.Id);
            return _mapper.Map<UserResponseDto>(CreatedUster);
        }
    }
}
