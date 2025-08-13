using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using EduShpere.Domain;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EduShpere.Application
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextService _httpContextService;
        public AuthService(IUserRepository userRepository, IConfiguration configuration, IHttpContextService httpContextService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _httpContextService = httpContextService;
        }
        public async Task<User> GetMe()
        {
            return await _httpContextService.GetAppUserAndThrow();
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
            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("Email",appUser.Email),
                    new Claim("FullName",appUser.Firstname),
                    new Claim("Id",appUser.Id.ToString()),
                    new Claim("UserName",appUser.Username),
                    new Claim("UserRole",appUser.Role.ToString()),
                }),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var accessTokenString = jwtTokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();
            //appUser.RefreshToken = refreshToken;
            //appUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            return new TokenModel
            {
                AccessToken = accessTokenString,
                RefreshToken = refreshToken
            };
        }
        public async Task<TokenModel> Login(LoginDto dto)
        {
            var user = await _userRepository.GetUserByUserName(dto.Username);

            if (user is null || user.Password != dto.Password)
            {
                throw new UnauthorizedException("Username or Password is not correct.");
            }

           

            var tokenModel = GenerateToken(user);
            return tokenModel;
        }
        public async Task<object> ImportUsers(IFormFile request)
        {
            if (request == null || request.Length == 0)
                throw new BadRequestException("File không hợp lệ.");

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
                    Name = fullName,
                    DateOfBirth = dob,
                    Email = email,
                    PassWord = password
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
            string[] vietnameseSigns = new string[] {
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

    }
}
