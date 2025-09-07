using EduShpere.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace EduShpere.Infrastructure.Security
{
    public static class PasswordHelper
    {
        private static readonly PasswordHasher<User> _hasher = new();

        public static string HashPassword(User user, string password)
        {
            return _hasher.HashPassword(user, password);
        }

        public static bool VerifyPassword(User user, string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}