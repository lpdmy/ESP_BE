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
using EduShpere.Infrastructure.Security;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

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

        public AuthService(IUserRepository userRepository, IConfiguration configuration, IHttpContextService httpContextService, IMapper mapper, IOneTimeLoginRepository oneTimeLoginRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _httpContextService = httpContextService;
            _mapper = mapper;
            _oneTimeLoginRepository = oneTimeLoginRepository;
            _emailService = emailService;
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

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Email", appUser.Email),
                    new Claim("FullName", appUser.FirstName),
                    new Claim("Id", appUser.Id.ToString()),
                    new Claim("UserName", appUser.Username),
                    new Claim("UserRole", appUser.Role.ToString())
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
            var user = await _userRepository.GetUserByUserName(dto.Username);

            if (user == null || user.Password != dto.Password)
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

        public async Task<bool> CreateUserAndGenerateOtlAsync(CreateUserDto dto)
        {
            if (await _userRepository.FindUserByEmail(dto.Email))
            {
                throw new BadRequestException(ErrorMessages.Auth.EmailAlreadyExists);
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                Password = PasswordHelper.HashPassword(new User(), "123"),
                Username = dto.Username
            };

            await _userRepository.AddAsync(user);

            var otlToken = await _oneTimeLoginRepository.CreateTokenAsync(user);
            var baseUrl = _configuration["AppSetting:FrontEndUrl"];
            var loginLink = $"{baseUrl}/auth/one-time-login?token={otlToken.Token}";
            await _emailService.SendEmailAsync(dto.Email, "Login Now", loginLink, true);

            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 1, string? search = null)
        {
            var users = await _userRepository.GetAllAsync();
            var list = users.Where(l => l.Role != UserRole.Admin);

            if (!string.IsNullOrEmpty(search))
            {
                list = list.Where(u =>
                    (!string.IsNullOrEmpty(u.Username) && u.Username.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(u.Email) && u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(u.FirstName) && u.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(u.LastName) && u.LastName.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            var pagedUsers = list
                .OrderBy(u => u.Id) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            return _mapper.Map<IEnumerable<UserDto>>(pagedUsers);
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
            user.UpdatedAt = DateTime.UtcNow;
            user.Role = dto.Role ?? user.Role;

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

            user.Password = PasswordHelper.HashPassword(user, newPassword);
            await _userRepository.UpdateAsync(user);
            await _oneTimeLoginRepository.MarkAsUsedAsync(otl);

            return GenerateToken(user);
        }
    }
}
