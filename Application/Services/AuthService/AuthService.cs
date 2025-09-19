using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ClosedXML.Excel;
using EduShpere.Application.DTOs.AuthDto;
using EduShpere.Application.DTOs.UserDto;
using EduShpere.Domain;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories.OneTimeLogin;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Infrastructure.Repositories.TeacherProfile;
using TeacherProfileEntity = EduShpere.Domain.Models.TeacherProfile;
using EduShpere.Infrastructure.Security;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using EduShpere.Application.Services;
using EduShpere.Application.DTOs.CommonDto;
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

        public AuthService(IUserRepository userRepository, IConfiguration configuration, IHttpContextService httpContextService, IMapper mapper, IOneTimeLoginRepository oneTimeLoginRepository, IEmailService emailService, IStudentProfileRepository studentProfileRepository, ITeacherProfileRepository teacherProfileRepository, IAuditService auditService, IPaginationService paginationService)
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
        }

        public async Task<UserDto> GetMe()
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            return _mapper.Map<UserDto>(user);
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

        private TokenModel GenerateToken(User appUser)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["AppSetting:SecretKey"];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

            // Convert role number to role name
            var roleName = appUser.Role switch
            {
                UserRole.Admin => "Admin",
                UserRole.Teacher => "Teacher", 
                UserRole.Student => "Student",
                _ => "Student"
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Email", appUser.Email),
                    new Claim("FullName", appUser.FirstName),
                    new Claim("Id", appUser.Id.ToString()),
                    new Claim("UserName", appUser.Username),
                    new Claim("UserRole", appUser.Role.ToString()),
                    new Claim(ClaimTypes.Role, roleName) // Add role claim for authorization
                }),
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

            return GenerateToken(user);
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
            await _emailService.SendEmailAsync(dto.Email, "Login Now", loginLink, true);

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
            return GenerateToken(user);
        }

        public async Task<PaginationResponseDto<UserDto>> GetAllUsersAsync(PaginationRequestDto paginationRequest)
        {
            var query = _userRepository.GetQueryable()
                .Where(u => u.Role != UserRole.Admin);

            var pagedResult = await _paginationService.GetPagedResultAsync(query, paginationRequest);

            return new PaginationResponseDto<UserDto>
            {
                Data = _mapper.Map<IEnumerable<UserDto>>(pagedResult.Data),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<bool> UpdateUserAsync(UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.Id);
            if (user == null)
                throw new NotFoundException(ErrorMessages.Auth.UserNotFound);

            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                !string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailExists = await _userRepository.FindUserByEmail(dto.Email);
                if (emailExists)
                {
                    throw new BadRequestException(ErrorMessages.Auth.EmailAlreadyExists);
                }

                user.Email = dto.Email;
            }

            user.Username = dto.Username ?? user.Username;
            user.FirstName = dto.FirstName ?? user.FirstName;
            user.LastName = dto.LastName ?? user.LastName;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
            user.Birthdate = dto.Birthdate ?? user.Birthdate;
            user.Address = dto.Address ?? user.Address;
            user.AvatarUrl = dto.AvatarUrl ?? user.AvatarUrl;
            user.Role = dto.Role ?? user.Role;

            _auditService.SetAuditFieldsForUpdate(user);
            await _userRepository.UpdateAsync(user);
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

            return GenerateToken(user);
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
            var resetLink = $"{baseUrl}/auth/reset-password?token={otlToken.Token}";

            // 4. Gửi email
            await _emailService.SendEmailAsync(
                user.Email,
                "Reset your password",
                $"Click the link below to reset your password:<br/><a href='{resetLink}'>Reset Password</a>",
                true
            );

            return GenerateToken(user);
        }

    }
}
