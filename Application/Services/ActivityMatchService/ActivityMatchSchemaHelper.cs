using System.Threading.Tasks;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Application.Services
{
    internal static class ActivityMatchSchemaHelper
    {
        private static bool _isPublishedColumnEnsured;

        public static async Task EnsureIsPublishedColumnExistsAsync(EduShpereDbContext dbContext)
        {
            if (_isPublishedColumnEnsured) return;

            const string sql = @"
IF COL_LENGTH('ActivityMatches', 'IsPublished') IS NULL
BEGIN
    ALTER TABLE ActivityMatches ADD IsPublished bit NOT NULL CONSTRAINT DF_ActivityMatches_IsPublished DEFAULT(0);
END";

            await dbContext.Database.ExecuteSqlRawAsync(sql);
            _isPublishedColumnEnsured = true;
        }
    }
}

