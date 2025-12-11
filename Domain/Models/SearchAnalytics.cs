using EduShpere.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Domain.Models
{
    public class SearchAnalytics : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Query { get; set; } = string.Empty;

        [Required]
        public SearchCategory Category { get; set; }

        public int SearchCount { get; set; } = 1;

        public int UniqueUsersCount { get; set; } = 1;

        public DateTime FirstSearched { get; set; } = DateTime.UtcNow;

        public DateTime LastSearched { get; set; } = DateTime.UtcNow;

        public bool IsTrending { get; set; } = false;

        public double TrendingScore { get; set; } = 0.0;
    }
}
