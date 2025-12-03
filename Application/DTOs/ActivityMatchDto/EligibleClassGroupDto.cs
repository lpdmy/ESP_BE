namespace EduShpere.Application.DTOs.ActivityMatchDto
{
    public class EligibleClassGroupDto
    {
        public int ClassGroupId { get; set; }
        public string? ClassGroupName { get; set; }
        public int? Grade { get; set; }
        public int AcademicYearId { get; set; }
        public string? AcademicYearName { get; set; }
        public int? HomeroomTeacherId { get; set; }
        public string? HomeroomTeacherName { get; set; }
    }

    public class EligibleClassGroupsByGradeDto
    {
        public int? Grade { get; set; }
        public int ClassCount { get; set; }
        public List<EligibleClassGroupDto> ClassGroups { get; set; } = new List<EligibleClassGroupDto>();
    }
}

