namespace EduShpere.Application.DTOs
{
    public class ImportStudentsResponseDto
    {
        public int Total { get; set; }
        public int Success { get; set; }
        public int Failed { get; set; }
        public List<ImportErrorDto> Errors { get; set; } = new();
        public List<ImportSuccessDto> SuccessItems { get; set; } = new();
    }

    public class ImportErrorDto
    {
        public Dictionary<string, string> Row { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class ImportSuccessDto
    {
        public int UserId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
