namespace EduShpere.Domain.Models
{
    /// <summary>
    /// Model thống kê cho ClassGroup ở tầng Domain/Infrastructure.
    /// Application layer sẽ map sang ClassGroupStatisticsDto khi cần.
    /// </summary>
    public class ClassGroupStatistics
    {
        public int TotalClasses { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int DeletedClasses { get; set; }
    }
}



