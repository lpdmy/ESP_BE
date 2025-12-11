using EduShpere.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Domain.Models
{
    public enum SearchCategory
    {
        All = 0,
        Users = 1,
        Posts = 2,
        Activities = 3,
        Clubs = 4,
        Hashtags = 5
    }

    public class SearchHistory : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Query { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [Required]
        public SearchCategory Category { get; set; }

        public int ResultCount { get; set; }

        public DateTime SearchedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
