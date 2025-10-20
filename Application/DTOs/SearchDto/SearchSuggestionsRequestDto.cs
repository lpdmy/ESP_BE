using System.ComponentModel.DataAnnotations;
using EduShpere.Domain.Models;

namespace EduShpere.Application.DTOs.SearchDto
{
    public class SearchSuggestionsRequestDto
    {
        [Required(ErrorMessage = "Query không được để trống")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Query phải có từ 1 đến 500 ký tự")]
        public string Query { get; set; } = string.Empty;

        [Range(1, 50, ErrorMessage = "Limit phải từ 1 đến 50")]
        public int Limit { get; set; } = 10;

        public SearchCategory? Category { get; set; }
    }
}
