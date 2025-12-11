namespace EduShpere.Application.DTOs.ClassGroupDto;

public class ClassGroupStatisticsDto
{
    public int TotalClasses { get; set; }
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int DeletedClasses { get; set; }
}

public class ClassGroupByGradeDto
{
    public int? Grade { get; set; } // VD: 10, 11, 12, null
    public string GradeName { get; set; } = null!; // VD: "Khối 10", "Khối 11", "Khối 12", "Không xác định"
    public int ClassCount { get; set; }
    public int StudentCount { get; set; }
    public List<ClassGroupDto> Classes { get; set; } = new List<ClassGroupDto>();
}

public class ClassGroupDashboardDto
{
    public ClassGroupStatisticsDto Statistics { get; set; } = null!;
    public List<ClassGroupByGradeDto> ClassesByGrade { get; set; } = new List<ClassGroupByGradeDto>();
}

public class ClassGroupFilterDto
{
    public string? Name { get; set; }
    public int? Grade { get; set; }
    public int? AcademicYearId { get; set; }
    public bool? IsDeleted { get; set; }
    public int? AcademicStartYear { get; set; }
}
